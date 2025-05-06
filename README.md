# GbxMedalTimeModifier

**GbxMedalTimeModifier** is a command-line utility to edit medal times in **Trackmania 2020** `.gbx` map files using [GBX.NET](https://github.com/BigBang1112/GBX.NET). It gives you control over Author, Gold, Silver, and Bronze times--something not currently possible with explorer.gbx.net.

## Features

* Set custom medal times for Author (AT), Gold, Silver, and Bronze.
* Use `_` to leave a medal unchanged.
* Use `auto` for Gold/Silver/Bronze to generate Trackmania-style defaults from AT.
    * Cannot use `auto` for Author Time.
* Supports batch processing via an optional Python script.

## Usage

### Command-Line Syntax

```
GbxMedalTimeModifier.exe <inputMapPath> <outputMapPath> <AT> <Gold> <Silver> <Bronze>
```
> Note: Inputs are only accepted in ms, 

### Parameters

* `<inputMapPath>`: Path to the source `.Gbx` map file.
* `<outputMapPath>`: Path to save the modified file.
* `<AT>`: Author Time in milliseconds (cannot be `auto`).
* `<Gold>`, `<Silver>`, `<Bronze>`: Medal times in milliseconds. Use:
    * `_` to leave unchanged.
    * `auto` to scale based on AT using Trackmania's default multipliers.

#### Auto Time Multipliers

* Gold = AT × 1.06
* Silver = AT × 1.20
* Bronze = AT × 1.50

### Example

```sh
GbxMedalTimeModifier.exe MyMap.Gbx MyMap_Modified.Gbx 60000 auto auto _
```

## Batch Processing (Optional)

A Python companion script is included for users who want to update **multiple maps at once**. To use it:

1. Make sure Python is installed.
2. Update the `INPUT_DIR` and `OUTPUT_DIR` in the script to point to your map folders.
3. Set the desired medal times in milliseconds in the script.
4. Run the script--it will apply the specified times to all `.Gbx` files in the input directory.