---
title: Changelog
description: UdonToolkit Changelog
---

UdonToolkit Changelog

---

## 2.0.0

### Summary

This release adds VCC support and splits the project into multiple packages.

## 1.2.1

### Summary

Hotfix for SyncedTrigger not working for late joiners with name-based access

[v1.2.1](https://github.com/orels1/UdonToolkit/tree/v1.2.1) (2022-11-30) (hotfix)

**This is the last release that uses U# v0.x and Legacy VRChat SDKs!**

All future releases will be using UdonSharp v1.x and rely on VCC SDKs. Please refer to the [VCC getting started guide](https://vcc.docs.vrchat.com/guides/getting-started) to learn more

### Bug Fixes

- Fixed a bug with SyncedTrigger not working correctly when utilizing name-based access control and late-joining the instance
- The ownership change now fires correctly

## 1.2.0

### Summary

Manual Sync, Cleanup and Prep for 2.x

[v1.2.0](https://github.com/orels1/UdonToolkit/tree/v1.2.0) (2022-11-05)

**This is the last release that uses U# v0.x and Legacy VRChat SDKs!**

All future releases will be using UdonSharp v1.x and rely on VCC SDKs. Please refer to the [VCC getting started guide](https://vcc.docs.vrchat.com/guides/getting-started) to learn more

### Enhancements

- Presentation System now uses Manual Sync
- The camera system has been removed completely, you can still download [the archive here](https://github.com/orels1/UTCameraSystem) if you need it
- Cleanups and updates overall

### Bug Fixes

- Fixed a bug in SyncedTrigger where it would not late-joiner sync when using `True` default state

## 1.1.2

### Summary

Area Trigger Composite Collider enhancement

[v1.1.2](https://github.com/orels1/UdonToolkit/tree/v1.1.2) (2021-09-08)

**DO NOT USE THIS ON UNITY 2018**

If you are still on 2018 - [use the older version](https://github.com/orels1/UdonToolkit/releases/tag/v1.0.0)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v1.1.1...v1.1.2)

### Enhancements

- Added internal composite colliders check
- Add composite collider check to Area Trigger [\#87](https://github.com/orels1/UdonToolkit/issues/87)

### Bug Fixes

- Added a `collidersIn` Clamp to the exit events (thx Lakuza)

## 1.1.1

Version 1.1.1 only existed as pre-release

## 1.1.0

### Summary

2019 Shader Updates

[v1.1.0](https://github.com/orels1/UdonToolkit/tree/v1.1.0) (2021-08-05)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v1.0.0...v1.1.0)

UdonToolkit was already compatible with 2019, but some shaders would throw a warning, so I fixed it in this release

**DO NOT USE THIS ON UNITY 2018**

If you are still on 2018 - [use the older version](https://github.com/orels1/UdonToolkit/releases/tag/v1.0.0)

### Upgrade Udon Sharp and VRCSDK to latest 2019 before installing

You can grab [VRCSDK here](https://vrchat.com/home/download) and [UdonSharp here](https://github.com/MerlinVR/UdonSharp/releases/tag/v0.20.2)

Here's how Toolkit looks in 2019. I am working on further UI updates to make inspectors even better! But for now you can continue using the UI you are used to

![image](https://user-images.githubusercontent.com/3798928/128309680-4cb83b83-3be7-4541-a1a7-a17fde82499d.png)

## 1.0.0

### Summary

1.0 is here 🎉

- All the inspectors got rewritten from scratch to be much much faster and more configurable
- Almost every attribute got improved, [check out the docs](https://ut.orels.sh/v/v1.x/attributes/attributes-list)
- `ListView` now supports any number of fields
- `Popup` now supports all the animation param types and can also cast things to `int` and `int[]`
- `TabGroup` attribute for even more complicated UIs :P
- `FoldoutGroup` attribute for even more nesting!
- Update loops are gone for delayed Universal Actions (less perf overhead)
- Many bugfixes related to player colliders/chairs etc etc
- Multiple new behaviours, including: Player Modifiers, Teleporter, Velocity Follower, On Respawn and others
- Synced Trigger behaviour using Manual Sync
- Access controls on all the triggers
- Presentation system for creating cool slide decks, same way the [Prefabs TLX](https://tlx.dev) does it

...and much much more

This has been **a long time coming**, it was tested by many people during the past couple of months and it seems to be stable enough to be released.

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.5.0...v1.0.0)

[Upgrading to 1.0.0 requires following these steps](https://ut.orels.sh/v/v1.x/extras/migration-to-v1.0.0) (link)

[If you have any issues, please reach out on discord!](https://discord.gg/aMdhyva7)

### Enhancements

- Add "revert to default" button [\#21](https://github.com/orels1/UdonToolkit/issues/21)
- Migrate to manual sync [\#82](https://github.com/orels1/UdonToolkit/issues/82)
- Rewrite delays to use SendCustomEventDelayed [\#75](https://github.com/orels1/UdonToolkit/issues/75)
- Add single use options to all the triggers [\#72](https://github.com/orels1/UdonToolkit/issues/72)
- Add access whitelists for all the triggers [\#71](https://github.com/orels1/UdonToolkit/issues/71)
- Add array QoL features [\#67](https://github.com/orels1/UdonToolkit/issues/67)
- Allow to pass a var to a TabGroup to save the currently selected tab into [\#64](https://github.com/orels1/UdonToolkit/issues/64)
- Support 3-4 elements in list view [\#20](https://github.com/orels1/UdonToolkit/issues/20)
- Refactor colors into a separate singleton [\#14](https://github.com/orels1/UdonToolkit/issues/14)
- Add support for boolean animator parameter to Universal Action [\#13](https://github.com/orels1/UdonToolkit/issues/13)
- Add Source Types to ShaderFeeder [\#16](https://github.com/orels1/UdonToolkit/issues/16)

### Bug Fixes

- The Synced Trrigger says "events" on the allowed users help box [\#81](https://github.com/orels1/UdonToolkit/issues/81)
- AreaTriggers are not backwards compatible [\#79](https://github.com/orels1/UdonToolkit/issues/79)
- Clean up the example code for the attributes to not beak builds [\#77](https://github.com/orels1/UdonToolkit/issues/77)
- Disabling an area trigger should clear the enter/exit counter [\#76](https://github.com/orels1/UdonToolkit/issues/76)
- Move camera to using distance checks instead of triggers [\#68](https://github.com/orels1/UdonToolkit/issues/68)
- Re-Add custom Add Method functionality to the ListViews [\#63](https://github.com/orels1/UdonToolkit/issues/63)

### Other Closed Issues

- Sound Occlusion Behaviour [\#18](https://github.com/orels1/UdonToolkit/issues/18)
- Document SoundOcclusion [\#17](https://github.com/orels1/UdonToolkit/issues/17)
- Add a PlayerModifiers behaviour [\#74](https://github.com/orels1/UdonToolkit/issues/74)
- Re-add UTEditor attribute for legacy support so it doesnt break anything [\#73](https://github.com/orels1/UdonToolkit/issues/73)
- Add OnRespawn trigger [\#70](https://github.com/orels1/UdonToolkit/issues/70)
- Add And document the Presentation System [\#65](https://github.com/orels1/UdonToolkit/issues/65)
- Add a Synced Trigger behaviour [\#62](https://github.com/orels1/UdonToolkit/issues/62)
- Velocity Tracking  Behaviour [\#61](https://github.com/orels1/UdonToolkit/issues/61)
- Move ListView attribute to the Modifier type [\#50](https://github.com/orels1/UdonToolkit/issues/50)
- Scene Start Trigger Behaviour [\#22](https://github.com/orels1/UdonToolkit/issues/22)
- Add basic guides [\#15](https://github.com/orels1/UdonToolkit/issues/15)
- Add And document the Presentation System [\#66](https://github.com/orels1/UdonToolkit/issues/66)
