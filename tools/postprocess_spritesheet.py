#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Remove checkerboard / solid backdrop from sprite sheets and optionally slice grid."""

from __future__ import annotations

import argparse
import json
import sys
from collections import deque
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("Error: pip install pillow", file=sys.stderr)
    sys.exit(1)


def color_distance(a: tuple[int, int, int], b: tuple[int, int, int]) -> float:
    return ((a[0] - b[0]) ** 2 + (a[1] - b[1]) ** 2 + (a[2] - b[2]) ** 2) ** 0.5


def is_checkerboard_gray(r: int, g: int, b: int, min_brightness: int = 200) -> bool:
    spread = max(r, g, b) - min(r, g, b)
    avg = (r + g + b) / 3
    return spread <= 18 and avg >= min_brightness


def is_background_pixel(
    r: int,
    g: int,
    b: int,
    *,
    key_color: tuple[int, int, int] | None,
    tolerance: float,
) -> bool:
    if key_color is not None:
        return color_distance((r, g, b), key_color) <= tolerance
    if is_checkerboard_gray(r, g, b):
        return True
    avg = (r + g + b) / 3
    return avg <= tolerance


def remove_checkerboard_pixels(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    pixels = rgba.load()
    w, h = rgba.size
    for y in range(h):
        for x in range(w):
            r, g, b, _ = pixels[x, y]
            if is_checkerboard_gray(r, g, b):
                pixels[x, y] = (r, g, b, 0)
    return rgba


def flood_remove_background(
    image: Image.Image,
    *,
    key_color: tuple[int, int, int] | None,
    tolerance: float,
) -> Image.Image:
    rgba = image.convert("RGBA")
    w, h = rgba.size
    pixels = rgba.load()
    visited = bytearray(w * h)
    queue: deque[tuple[int, int]] = deque()

    def idx(x: int, y: int) -> int:
        return y * w + x

    def try_seed(x: int, y: int) -> None:
        i = idx(x, y)
        if visited[i]:
            return
        r, g, b, _ = pixels[x, y]
        if is_background_pixel(r, g, b, key_color=key_color, tolerance=tolerance):
            visited[i] = 1
            queue.append((x, y))

    for x in range(w):
        try_seed(x, 0)
        try_seed(x, h - 1)
    for y in range(h):
        try_seed(0, y)
        try_seed(w - 1, y)

    while queue:
        x, y = queue.popleft()
        pixels[x, y] = (pixels[x, y][0], pixels[x, y][1], pixels[x, y][2], 0)
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if nx < 0 or nx >= w or ny < 0 or ny >= h:
                continue
            i = idx(nx, ny)
            if visited[i]:
                continue
            r, g, b, _ = pixels[nx, ny]
            if is_background_pixel(r, g, b, key_color=key_color, tolerance=tolerance):
                visited[i] = 1
                queue.append((nx, ny))

    return rgba


def cleanup_gray_fringe(image: Image.Image, passes: int = 2) -> Image.Image:
    """Remove light-gray checkerboard residue touching transparent pixels."""
    rgba = image.convert("RGBA")
    for _ in range(passes):
        pixels = rgba.load()
        w, h = rgba.size
        to_clear: list[tuple[int, int]] = []
        for y in range(h):
            for x in range(w):
                r, g, b, a = pixels[x, y]
                if a == 0 or not is_checkerboard_gray(r, g, b, min_brightness=190):
                    continue
                for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                    if 0 <= nx < w and 0 <= ny < h and pixels[nx, ny][3] == 0:
                        to_clear.append((x, y))
                        break
        for x, y in to_clear:
            pixels[x, y] = (0, 0, 0, 0)
    return rgba


def trim_cell(cell: Image.Image, pad: int = 2) -> Image.Image:
    bbox = cell.split()[-1].getbbox()
    if bbox is None:
        return cell
    left, top, right, bottom = bbox
    left = max(0, left - pad)
    top = max(0, top - pad)
    right = min(cell.size[0], right + pad)
    bottom = min(cell.size[1], bottom + pad)
    return cell.crop((left, top, right, bottom))


def fit_cell(cell: Image.Image, size: int) -> Image.Image:
    trimmed = trim_cell(cell)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    tw, th = trimmed.size
    scale = min((size - 4) / tw, (size - 4) / th)
    nw = max(1, round(tw * scale))
    nh = max(1, round(th * scale))
    resized = trimmed.resize((nw, nh), Image.Resampling.NEAREST)
    ox = (size - nw) // 2
    oy = (size - nh) // 2
    canvas.paste(resized, (ox, oy), resized)
    return canvas


def slice_grid(
    image: Image.Image,
    cols: int,
    rows: int,
    out_dir: Path,
    frame_size: int | None,
) -> dict:
    w, h = image.size
    cell_w = w // cols
    cell_h = h // rows
    out_dir.mkdir(parents=True, exist_ok=True)

    row_names = ["idle", "walk", "action_a", "action_b", "action_c", "action_d"]
    meta = {"cols": cols, "rows": rows, "cell_w": cell_w, "cell_h": cell_h, "frames": []}

    for row in range(rows):
        row_name = row_names[row] if row < len(row_names) else f"row_{row}"
        for col in range(cols):
            left = col * cell_w
            top = row * cell_h
            cell = image.crop((left, top, left + cell_w, top + cell_h))
            if frame_size:
                cell = fit_cell(cell, frame_size)
            name = f"{row_name}_{col:02d}.png"
            path = out_dir / name
            cell.save(path, format="PNG")
            meta["frames"].append({"file": name, "row": row, "col": col, "animation": row_name})

    meta_path = out_dir / "sheet_meta.json"
    meta_path.write_text(json.dumps(meta, indent=2, ensure_ascii=False), encoding="utf-8")
    return meta


def main() -> int:
    parser = argparse.ArgumentParser(description="Post-process sprite sheet background and slice.")
    parser.add_argument("input", type=Path)
    parser.add_argument("output", type=Path, help="Processed sheet PNG")
    parser.add_argument("--backup", type=Path, default=None)
    parser.add_argument("--cols", type=int, default=5)
    parser.add_argument("--rows", type=int, default=6)
    parser.add_argument("--frames-dir", type=Path, default=None)
    parser.add_argument("--frame-size", type=int, default=128)
    parser.add_argument("--tolerance", type=float, default=35.0)
    parser.add_argument("--no-slice", action="store_true")
    args = parser.parse_args()

    if not args.input.is_file():
        print(f"Error: not found {args.input}", file=sys.stderr)
        return 1

    if args.backup:
        args.backup.parent.mkdir(parents=True, exist_ok=True)
        Image.open(args.input).save(args.backup)

    image = Image.open(args.input)
    result = remove_checkerboard_pixels(image)
    result = flood_remove_background(result, key_color=None, tolerance=args.tolerance)
    result = cleanup_gray_fringe(result, passes=3)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    result.save(args.output, format="PNG")

    alpha = result.split()[-1]
    transparent = sum(1 for v in alpha.get_flattened_data() if v == 0)
    total = result.size[0] * result.size[1]
    print(f"Saved {args.output} ({result.size[0]}x{result.size[1]} RGBA, transparent={transparent}/{total})")

    if not args.no_slice and args.frames_dir:
        meta = slice_grid(result, args.cols, args.rows, args.frames_dir, args.frame_size)
        print(f"Sliced {len(meta['frames'])} frames -> {args.frames_dir}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
