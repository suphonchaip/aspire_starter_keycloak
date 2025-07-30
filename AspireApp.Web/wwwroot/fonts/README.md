# Srinakharinwirot Font Files

Please add the following Srinakharinwirot font files to this directory:

1. `srinakharinwirot-regular.eot` - For Internet Explorer support
2. `srinakharinwirot-regular.woff` - Web Open Font Format (recommended)
3. `srinakharinwirot-regular.ttf` - TrueType Font (fallback)

## How to obtain the fonts:

- Download from the official Srinakharinwirot University website
- Or convert from existing TTF files using online font converters
- Ensure you have proper licensing for web usage

## File structure should be:

```
static/fonts/
├── README.md (this file)
├── srinakharinwirot-regular.eot
├── srinakharinwirot-regular.woff
└── srinakharinwirot-regular.ttf
```

## Usage in components:

- Font is automatically applied globally
- Use `font-srinakharinwirot` or `font-thai` Tailwind classes for specific elements
- Font will fallback to Sarabun > Noto Sans Thai > Noto Sans > sans-serif if files are missing
