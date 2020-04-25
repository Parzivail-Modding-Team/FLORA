# FLORA
### Fabric Lightweight Obfuscation Remapping Assistant

## What is FLORA?
FLORA is an all-in-one utility for navigating and applying the Yarn mappings outside of a Gradle environment. From finding the children of an obfuscated class to remapping an entire source jar, we've got you covered.

## Usage

FLORA has two usage modes: The command line interface, and interactive mode. If you're just remapping jars, you'll likely want to use the command line interface.

In both modes, FLORA keeps a local database of the mappings you use so that you'll only need internet access once to use a specific mapping set.

### Command Line Interface

The command line interface is simple. FLORA takes two arguments, the input archive, and, optionally, the output directory. A number of flags are available.

```
FLORA.exe <input archive> [output directory] [-v version] [-t tinyfile] 
```

#### Input Archive

The input archive does not have to be a jarfile, any archive which is compatible with Microsoft's ZIP format is acceptable.

If no `sources` jar is available, you can create a source archive by decompiling the compiled jar with a tool like [JD-GUI](http://java-decompiler.github.io/) if the license of the mod permits it.

#### Output Directory

This is where the entireity of the contents of the input archive will be extracted to. If the file is not a Java file, it is directly copied, otherwise it is remapped with the given mapping version. If no output directory is specified, one will be created based on the input archive filename.

#### Version flag

The version flag denotes the Yarn mapping versions to use. A command using this flag could be structured as:

```
FLORA.exe my-mod-sources.jar -v "1.15.2+build.7"
```

##### Version Autodetection

If no version flag is specified, FLORA attempts to automatically determine a compatible mapping version based on the target game version of the input archive. For this to work, the input archive needs to have a `fabric.mod.json` in the root directory which contains a `minecraft` key in the `depends` block.

#### Tinyfile flag

The tiny flag allows you to directly specify a `.tiny` file that contains the mappings you wish to use to remap the input archive. A command using this flag could be structured as:

```
FLORA.exe my-mod-sources.jar -t my-mappings.tiny
```

### Interactive Mode

Interactive mode is designed to be as quick and easy of a reference as possible while still maintaining the power of the command line interface. The interactive mode supports a number of commands and features that the command line interface does not.

The interactive mode supports a rich help interface to provide details about each command.

#### Getting Started

Detailed help for any of the commands mentioned here can be found by accessing the help interface, `help [command]`.

##### Selecting mappings

First, you'll want to select a mapping source using the `mapsrc` command. This command can be run at any time to change the mappings currently used in your environment.

If you want to use the mapping version `1.15.2+build.7`, you'd run

```
mapsrc 1.15.2+build.7
```

However, if you had a mapping file you want to use called `my-mappings.tiny`, you'd run

```
mapsrc my-mappings.tiny
```

If you are unsure which Yarn versions are available to you, you can find some using the `yarnver` command.

```
yarnver 1.15.2
```

##### Searching for classes, fields, or methods

Get information about the mappings of classes, fields, or methods with the `search` command.

```
search class_1324
```

#### Finding the children of a given class

Find the explicitly defined child classes, fields, and methods of the given class with the `children` command.

```
children ChestBlockEntity
```

#### Mapping the intermediate names in a string

You do not need to provide your string in quotations. Anything after the `mapstr` command will be considered your input string.

```
mapstr this.horseBaseEntity.method_6127().method_6205(class_1612.field_7357).method_6194()
```

### Mapping an entire archive

Much like the command line interface, an entire archive can be remapped using the `mapjar` command.

```
mapjar my-mod-sources.jar my-mod-sources-mapped
```

The `mapjar` command does not accept the `-v` or `-t` options, as it uses the mappings specified by the `mapsrc` command.


## Screenshots

![Remapping a jar](https://i.imgur.com/OVdAXzy.png)

![The search and children commands](https://i.imgur.com/FmotXMF.png)