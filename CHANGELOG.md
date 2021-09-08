# Changelog

## [v1.1.2](https://github.com/orels1/UdonToolkit/tree/v1.1.2) (2021-09-08)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v1.1.1...v1.1.2)

Added internal composite colliders check

- Added a `collidersIn` Clamp to the exit events (thx Lakuza)

**Implemented enhancements:**

- Add composite collider check to Area Trigger [\#87](https://github.com/orels1/UdonToolkit/issues/87)

## [v1.1.1](https://github.com/orels1/UdonToolkit/tree/v1.1.1) (2021-08-19)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v1.1.0...v1.1.1)

Added package json
UdonToolkit was already compatible with 2019, but some shaders would throw a warning, so I fixed it in this release

# DO NOT USE THIS ON UNITY 2018

If you are still on 2018 - [use the older version](https://github.com/orels1/UdonToolkit/releases/tag/v1.0.0)

## Upgrade Udon Sharp and VRCSDK to latest 2019 before installing

You can grab [VRCSDK here](https://vrchat.com/home/download) and [UdonSharp here](https://github.com/MerlinVR/UdonSharp/releases/tag/v0.20.2)

Here's how Toolkit looks in 2019. I am working on further UI updates to make inspectors even better! But for now you can continue using the UI you are used to

![image](https://user-images.githubusercontent.com/3798928/128309680-4cb83b83-3be7-4541-a1a7-a17fde82499d.png)


## [v1.1.0](https://github.com/orels1/UdonToolkit/tree/v1.1.0) (2021-08-05)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v1.0.0...v1.1.0)

**Implemented enhancements:**

- Add Source Types to ShaderFeeder [\#16](https://github.com/orels1/UdonToolkit/issues/16)

**Closed issues:**

- Add And document the Presentation System [\#66](https://github.com/orels1/UdonToolkit/issues/66)

## [v1.0.0](https://github.com/orels1/UdonToolkit/tree/v1.0.0) (2021-07-12)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.5.0...v1.0.0)

# [Upgrading to 1.0.0 requires following these steps](https://ut.orels.sh/v/v1.x/extras/migration-to-v1.0.0) (link)

## 1.0 is here! 🎉

Here's a shortlist of changes:

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

### [If you have any issues, please reach out on discord!](https://discord.gg/aMdhyva7)

**Implemented enhancements:**

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

**Fixed bugs:**

- The Synced Trrigger says "events" on the allowed users help box [\#81](https://github.com/orels1/UdonToolkit/issues/81)
- AreaTriggers are not backwards compatible [\#79](https://github.com/orels1/UdonToolkit/issues/79)
- Clean up the example code for the attributes to not beak builds [\#77](https://github.com/orels1/UdonToolkit/issues/77)
- Disabling an area trigger should clear the enter/exit counter [\#76](https://github.com/orels1/UdonToolkit/issues/76)
- Move camera to using distance checks instead of triggers [\#68](https://github.com/orels1/UdonToolkit/issues/68)
- Re-Add custom Add Method functionality to the ListViews [\#63](https://github.com/orels1/UdonToolkit/issues/63)

**Closed issues:**

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

## [v0.5.0](https://github.com/orels1/UdonToolkit/tree/v0.5.0) (2021-01-11)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.4.6...v0.5.0)

![](https://cdn.vrchat.sh/ut/promo/Flight%20System%20Banner%20Wide.png)

This release adds my flight system to Udon Toolkit! [Check out the documentation to learn more](https://ut.orels.sh/systems/flight-system)

**Implemented enhancements:**

- Add FlightSystem [\#59](https://github.com/orels1/UdonToolkit/issues/59)

**Closed issues:**

- Add version.txt [\#58](https://github.com/orels1/UdonToolkit/issues/58)

## [v0.4.6](https://github.com/orels1/UdonToolkit/tree/v0.4.6) (2020-12-02)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.4.5...v0.4.6)

I introduced a bug in `0.4.5` that caused a project build to crash if you had demo assets imported. This release fixes it

**Fixed bugs:**

- CustomUISample crashes build process [\#56](https://github.com/orels1/UdonToolkit/issues/56)

## [v0.4.5](https://github.com/orels1/UdonToolkit/tree/v0.4.5) (2020-11-29)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.4.4...v0.4.5)

- Fixed Player collision in Area Trigger
- Added backwards compat for old code
Drag & Drop is now implemented as a part of array rendering. You can now drag a bunch of game objects and the proper components will be picked up and put into the relevant fields of your arrays and ListViews.

For List Views it will try to look for the components specified in list view types to populate either or both of the fields.

This release also significantly simplifies the Skybox Adjustment behaviour and fixes a bug with OnValueChange handlers not being passed correct parameters in some cases.

Old `UTTestController` has also been replaced by a `CustomUISample` that now exists inside the demo folder. It should provide an up-to-date example of a fairly complex UI setup

**Implemented enhancements:**

- Add support for Drag & Drop on Arrays and ListViews [\#54](https://github.com/orels1/UdonToolkit/issues/54)

**Closed issues:**

- Remove the `Interact` trigger from UdonBehaviour Event autofill [\#53](https://github.com/orels1/UdonToolkit/issues/53)
- Move the detailed installation instructions to the Readme [\#52](https://github.com/orels1/UdonToolkit/issues/52)

## [v0.4.4](https://github.com/orels1/UdonToolkit/tree/v0.4.4) (2020-10-07)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.4.3...v0.4.4)

## [v0.4.3](https://github.com/orels1/UdonToolkit/tree/v0.4.3) (2020-09-25)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.4.2...v0.4.3)

## [v0.4.2](https://github.com/orels1/UdonToolkit/tree/v0.4.2) (2020-09-14)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.4.1...v0.4.2)

Only bug fixes this time, nothing to see here 👀 

**Fixed bugs:**

- "Exception of type 'UnityEngine.ExitGUIException' was thrown." error happens whenever you click the circle next to a reference [\#48](https://github.com/orels1/UdonToolkit/issues/48)
- Using a custom label on a toggle attribute throws an error when doing a VRC build [\#47](https://github.com/orels1/UdonToolkit/issues/47)
- Helpboxes applied to arrays do not respect the provided methodname [\#46](https://github.com/orels1/UdonToolkit/issues/46)

## [v0.4.1](https://github.com/orels1/UdonToolkit/tree/v0.4.1) (2020-09-14)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.4.0...v0.4.1)

Mostly bugfixes and small improvements to [Popup](https://github.com/orels1/UdonToolkit/wiki/Attributes#popup) and [OnValueChanged](https://github.com/orels1/UdonToolkit/wiki/Attributes#onvaluechanged) attributes.

Added new class-level attributes: [OnBeforeEditor](https://github.com/orels1/UdonToolkit/wiki/Attributes#onbeforeeditor), [OnAfterEditor](https://github.com/orels1/UdonToolkit/wiki/Attributes#onaftereditor) and [OnValuesChanged](https://github.com/orels1/UdonToolkit/wiki/Attributes#onvalueschanged)

Added new field-level attribute: [Disabled](https://github.com/orels1/UdonToolkit/wiki/Attributes#disabled)

All attributes also have inline documentation for better IDE support now too 🎉 

**Implemented enhancements:**

- Add a "Show Default Inspector" button for ez debugging [\#43](https://github.com/orels1/UdonToolkit/issues/43)

**Fixed bugs:**

- Adding `\[ListView\]` to existing arrays of different lengths breaks the editor [\#39](https://github.com/orels1/UdonToolkit/issues/39)
- Duplicate Section Header names case them to not appear [\#38](https://github.com/orels1/UdonToolkit/issues/38)
- Popup crashes when used on a regular array [\#42](https://github.com/orels1/UdonToolkit/issues/42)

**Closed issues:**

- Reorderable List causes replacement of values [\#36](https://github.com/orels1/UdonToolkit/issues/36)
- Add migration docs [\#33](https://github.com/orels1/UdonToolkit/issues/33)
- Document new requirements for UT [\#27](https://github.com/orels1/UdonToolkit/issues/27)
- Add \[Disabled\] and \[DisabledIf\] attributes for automatically controlled fields [\#41](https://github.com/orels1/UdonToolkit/issues/41)
- Add \[OnBeforeEditor\], \[OnAfterEditor\], \[OnValuesChanged\] attributes with SerializedObject [\#40](https://github.com/orels1/UdonToolkit/issues/40)
- Add explanation of the Attribute stack to the docs [\#37](https://github.com/orels1/UdonToolkit/issues/37)
- Add inline documentation for Attributes [\#35](https://github.com/orels1/UdonToolkit/issues/35)

**Merged pull requests:**

- 0.4.1 Release [\#45](https://github.com/orels1/UdonToolkit/pull/45) ([orels1](https://github.com/orels1))

## [v0.4.0](https://github.com/orels1/UdonToolkit/tree/v0.4.0) (2020-09-06)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.3.4...v0.4.0)

**Implemented enhancements:**

- Make events generate buttons automatically [\#28](https://github.com/orels1/UdonToolkit/issues/28)

**Closed issues:**

- Update UTTestController to be UTTestBeahviour [\#32](https://github.com/orels1/UdonToolkit/issues/32)
- Migrate old Popups to the new U\# friendly Popup attributes [\#31](https://github.com/orels1/UdonToolkit/issues/31)
- Split popup source into separate popups to avoid Enums [\#30](https://github.com/orels1/UdonToolkit/issues/30)
- Add an attribute to request udon behaviour arrays specifically [\#29](https://github.com/orels1/UdonToolkit/issues/29)
- Update `Button` to call custom event by default [\#26](https://github.com/orels1/UdonToolkit/issues/26)
- Remove the ControlledBehaviour attribute [\#25](https://github.com/orels1/UdonToolkit/issues/25)
- Update UTEditor to support U\# 0.18 [\#24](https://github.com/orels1/UdonToolkit/issues/24)
- Add Build-Safe versions of attributes [\#23](https://github.com/orels1/UdonToolkit/issues/23)
- Change the Fog code to interpolate [\#12](https://github.com/orels1/UdonToolkit/issues/12)
- Remove controller generator 😭 [\#11](https://github.com/orels1/UdonToolkit/issues/11)
- Document the `\#if` requirements [\#10](https://github.com/orels1/UdonToolkit/issues/10)
- Document the Removal of UTController [\#9](https://github.com/orels1/UdonToolkit/issues/9)
- Merge Controller code into the UdonSharpBehaviours [\#8](https://github.com/orels1/UdonToolkit/issues/8)
- Add support for PlayMode value adjustment [\#7](https://github.com/orels1/UdonToolkit/issues/7)
- Add back sync to UTController on first editor render [\#6](https://github.com/orels1/UdonToolkit/issues/6)
- Improve the `Button` rendering [\#5](https://github.com/orels1/UdonToolkit/issues/5)
- Document manual copy paste value buttons [\#4](https://github.com/orels1/UdonToolkit/issues/4)
- Document Edit mode buttons [\#3](https://github.com/orels1/UdonToolkit/issues/3)
- Remove PostProcessing requirement for compiling and add warning [\#2](https://github.com/orels1/UdonToolkit/issues/2)
- Extract the shader Properties as a popup source [\#1](https://github.com/orels1/UdonToolkit/issues/1)

**Merged pull requests:**

- U\# v0.18 Upgrade [\#34](https://github.com/orels1/UdonToolkit/pull/34) ([orels1](https://github.com/orels1))

## [v0.3.4](https://github.com/orels1/UdonToolkit/tree/v0.3.4) (2020-08-26)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.3.3...v0.3.4)

## [v0.3.3](https://github.com/orels1/UdonToolkit/tree/v0.3.3) (2020-08-26)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.3.2...v0.3.3)

## [v0.3.2](https://github.com/orels1/UdonToolkit/tree/v0.3.2) (2020-08-24)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.3.1...v0.3.2)

## [v0.3.1](https://github.com/orels1/UdonToolkit/tree/v0.3.1) (2020-08-21)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.3.0...v0.3.1)

## [v0.3.0](https://github.com/orels1/UdonToolkit/tree/v0.3.0) (2020-08-10)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.2.1...v0.3.0)

## [v0.2.1](https://github.com/orels1/UdonToolkit/tree/v0.2.1) (2020-08-03)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.2.0...v0.2.1)

## [v0.2.0](https://github.com/orels1/UdonToolkit/tree/v0.2.0) (2020-07-31)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.7...v0.2.0)

## [v0.1.7](https://github.com/orels1/UdonToolkit/tree/v0.1.7) (2020-07-28)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.6...v0.1.7)

## [v0.1.6](https://github.com/orels1/UdonToolkit/tree/v0.1.6) (2020-07-26)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.5...v0.1.6)

## [v0.1.5](https://github.com/orels1/UdonToolkit/tree/v0.1.5) (2020-07-24)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.4...v0.1.5)

## [v0.1.4](https://github.com/orels1/UdonToolkit/tree/v0.1.4) (2020-07-23)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.3...v0.1.4)

## [v0.1.3](https://github.com/orels1/UdonToolkit/tree/v0.1.3) (2020-07-23)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.2...v0.1.3)

## [v0.1.2](https://github.com/orels1/UdonToolkit/tree/v0.1.2) (2020-07-22)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.1...v0.1.2)

## [v0.1.1](https://github.com/orels1/UdonToolkit/tree/v0.1.1) (2020-07-22)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/v0.1.0...v0.1.1)

## [v0.1.0](https://github.com/orels1/UdonToolkit/tree/v0.1.0) (2020-07-22)

[Full Changelog](https://github.com/orels1/UdonToolkit/compare/07fa7228c2e7b16cd3f5e6e9b4dc8dfd2aa9e735...v0.1.0)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
