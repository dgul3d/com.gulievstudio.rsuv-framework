# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-04-13

### Added

- Split generated bindings output into separate C# and HLSL directories to support stricter project structure separation when needed.

## [1.0.0] - 2026-04-04

### Added

- Initial package structure for schema-driven Renderer Shader User Value authoring.
- `RSUVSchema` assets with validation and automatic 32-bit field packing.
- Runtime packing helpers through `RSUVRuntimeState`.
- `RSUVRendererValueWriter` component for applying packed values to target renderers.
- Shared shader decode helpers in `ShaderLibrary/RSUVCore.hlsl`.
- Generated per-schema HLSL wrappers for handwritten shaders and Shader Graph custom function nodes.
- Generated per-schema C# bindings with typed `RSUVFieldKey<TValue>` access and extension-style writer setters.
- Inspector tooling for schema validation, binding generation, and editing schema-backed runtime values.