# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.5] - 2026-06-24

### Fixed

- Guarded `RSUVRendererValueWriter` custom inspector against null targets during scene load and unload, preventing `SerializedObjectNotCreatableException` on play/enter.

## [1.1.4] - 2026-06-24

### Fixed

- Stopped `RSUVRendererValueWriter` from marking prefabs dirty when opened by removing unconditional inspector refresh on enable, syncing serialized fields only when the schema layout actually changed, and avoiding serialized writes during editor runtime initialization.

## [1.1.3] - 2026-06-18

### Fixed

- Split schema validation into structural and project-wide scopes to avoid `AssetDatabase` scans during inspector repaints, fixing editor lag and freezes while editing `RSUVSchema` assets.

## [1.1.2] - 2026-06-18

### Fixed

- Guarded schema naming-prefix validation against `AssetDatabase` access during editor refresh and compilation to prevent import-time race conditions.
- `RSUVRendererValueWriter` inspector now refreshes schema values immediately when the assigned schema layout changes, including edits made in a separate schema inspector tab.

## [1.1.1] - 2026-05-27

### Fixed

- Optimized `RSUVRendererValueWriter` hot updates so immediate field setters reuse cached schema/runtime state, avoid per-call state rebuilds, and only fall back to full serialized reapply when deferred changes are pending.
- Reduced `ApplySerializedValues` overhead by reusing runtime state, avoiding repeated serialized field list scans, and skipping redundant default-state initialization during full rebuilds.

## [1.1.0] - 2026-06-25

### Changed

- Schema validation now requires each schema to use a unique sanitized naming prefix so generated APIs cannot collide in C# nor in HLSL.
- Binding generation now emits shared `RSUVBindings.hlsl` and `RSUVBindings.cs` files per output directory instead of per-schema generated files. `RSUVBindings.cs` now lives in the `RSUVFramework` namespace instead of `RSUVFramework.Generated`.
- HLSL and C# generated output directories now default to `Assets/Art/Shaders/Include` and `Assets/Scripts/Generated`.
- Generated output directories are now configured through a shared `RSUVGenerationSettings` asset instead of per-schema settings on `RSUVSchema` assets.
- Reworked demo scene to show multiple schemas usage and the example of shader that is shared across schemas.

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