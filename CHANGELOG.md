# Changelog

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
