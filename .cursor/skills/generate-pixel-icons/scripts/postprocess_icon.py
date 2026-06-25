#!/usr/bin/env python3
"""Post-process generated icons: remove background, fit to square, export RGBA PNG."""

from __future__ import annotations

import argparse
import sys
from collections import deque
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("Error: Pillow required. Install with: pip install pillow", file=sys.stderr)
    sys.exit(1)


def parse_color(value: str) -> tuple[int, int, int]:
    value = value.strip().lower()
    named = {
        "black": (0, 0, 0),
        "white": (255, 255, 255),
        "magenta": (255, 0, 255),
        "green": (0, 255, 0),
    }
    if value in named:
        return named[value]
    if value.startswith("#") and len(value) == 7:
        return (
            int(value[1:3], 16),
            int(value[3:5], 16),
            int(value[5:7], 16),
        )
    raise argparse.ArgumentTypeError(f"Invalid color: {value}")


def color_distance(a: tuple[int, int, int], b: tuple[int, int, int]) -> float:
    return ((a[0] - b[0]) ** 2 + (a[1] - b[1]) ** 2 + (a[2] - b[2]) ** 2) ** 0.5


def is_checkerboard_gray(r: int, g: int, b: int, min_brightness: int = 205) -> bool:
    spread = max(r, g, b) - min(r, g, b)
    avg = (r + g + b) / 3
    return spread <= 15 and avg >= min_brightness


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


def content_bbox(image: Image.Image) -> tuple[int, int, int, int] | None:
    alpha = image.convert("RGBA").split()[-1]
    return alpha.getbbox()


def fit_to_square(image: Image.Image, size: int, padding: int = 8) -> Image.Image:
    bbox = content_bbox(image)
    if bbox is None:
        return Image.new("RGBA", (size, size), (0, 0, 0, 0))

    cropped = image.crop(bbox)
    inner = max(1, size - padding * 2)
    src_w, src_h = cropped.size
    scale = min(inner / src_w, inner / src_h)
    new_w = max(1, round(src_w * scale))
    new_h = max(1, round(src_h * scale))
    resized = cropped.resize((new_w, new_h), Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    offset = ((size - new_w) // 2, (size - new_h) // 2)
    canvas.paste(resized, offset, resized)
    return canvas


def process_icon(
    image: Image.Image,
    *,
    size: int,
    key_color: tuple[int, int, int] | None,
    tolerance: float,
    padding: int,
    do_resize: bool,
) -> Image.Image:
    keyed = remove_checkerboard_pixels(image)
    keyed = flood_remove_background(keyed, key_color=key_color, tolerance=tolerance)
    if not do_resize:
        return keyed
    return fit_to_square(keyed, size, padding=padding)


def main() -> int:
    parser = argparse.ArgumentParser(description="Remove background and fit icon to square canvas.")
    parser.add_argument("input", type=Path, help="Source image path")
    parser.add_argument("output", type=Path, help="Output PNG path")
    parser.add_argument("--size", type=int, default=128, help="Target square size (default: 128)")
    parser.add_argument(
        "--bg",
        type=parse_color,
        default=None,
        help="Force a single background color to remove (black/white/#RRGGBB). Auto-detect if omitted.",
    )
    parser.add_argument("--tolerance", type=float, default=30.0, help="Background match tolerance (default: 30)")
    parser.add_argument("--padding", type=int, default=8, help="Inner padding when fitting to square (default: 8)")
    parser.add_argument("--no-resize", action="store_true", help="Only remove background, keep original size")
    args = parser.parse_args()

    if not args.input.is_file():
        print(f"Error: input not found: {args.input}", file=sys.stderr)
        return 1

    image = Image.open(args.input)
    result = process_icon(
        image,
        size=args.size,
        key_color=args.bg,
        tolerance=args.tolerance,
        padding=args.padding,
        do_resize=not args.no_resize,
    )
    args.output.parent.mkdir(parents=True, exist_ok=True)
    result.save(args.output, format="PNG")

    alpha = result.split()[-1]
    transparent = sum(1 for v in alpha.get_flattened_data() if v == 0)
    print(
        f"Saved {args.output} ({result.size[0]}x{result.size[1]}, RGBA, "
        f"transparent={transparent}/{result.size[0] * result.size[1]})"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
