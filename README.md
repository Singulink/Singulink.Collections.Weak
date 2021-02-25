# Singulink.Collections.Weak

[![Join the chat](https://badges.gitter.im/Singulink/community.svg)](https://gitter.im/Singulink/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![View nuget packages](https://img.shields.io/nuget/v/Singulink.Collections.Weak.svg)](https://www.nuget.org/packages/Singulink.Collections.Weak/)
[![Build and Test](https://github.com/Singulink/Singulink.Collections.Weak/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink.Collections.Weak/actions?query=workflow%3A%22build+and+test%22)

**Singulink.Collections.Weak** provides collection classes that store weak references to the values so that they can be garbage collected when they are no longer needed.

### About Singulink

*Shameless plug*: We are a small team of engineers and designers dedicated to building beautiful, functional and well-engineered software solutions. We offer very competitive rates as well as fixed-price contracts and welcome inquiries to discuss any custom development / project support needs you may have.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

## Installation

The package is available on NuGet - simply install the `Singulink.Collections.Weak` package.

**Supported Runtimes**: Anywhere .NET Standard 2.1+ is supported, including:
- .NET Core 3.0+
- Mono 6.4+
- Xamarin.iOS 12.16+
- Xamarin.Android 10.0+

## Usage

The following collections are included in the package:
- `WeakCollection`: Collection of weakly referenced values that keeps items in an undefined order.
- `WeakList`: Collection of weakly referenced values that maintains relative insertion order.
- `WeakValueDictionary`: Collection of keys and weakly referenced values.

The values returned by the collections will never be `null` - if the value was garbage collected then is skipped over for all operations.

`WeakCollection` and `WeakDictionary` remove entries for values that were garbage collected as they are encountered, so if you regularly enumerate over all the values then additional cleaning is not necessary. `WeakList` needs to have the `Clean()` method called periodically to remove the entries for garbage collected values from memory.

Additionally, all the collections have the following properties to help with cleaning:
- `AutoCleanAddCount`: Sets the number of `Add` operations that automatically triggers the `Clean()` method to run.
- `AddCountSinceLastClean`: Gets the number of add operations that have been performed since the last cleaning. Can be used by your code to implement more complex custom logic for triggering a cleaning.
- `TrimExcessDuringClean`: Sets a value indicating whether to automatically call `TrimExcess()` whenever `Clean()` is called. Useful for keeping memory usage to a minimum when relying on the automatic cleaning functionality.

## Further Reading

You can view the fully documented API on the [project documentation site](https://www.singulink.com/Docs/Singulink.Collections.Weak/api/Singulink.Collections.html).
