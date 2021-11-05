# Singulink.Collections.Weak

[![Chat on Discord](https://img.shields.io/discord/906246067773923490)](https://discord.gg/EkQhJFsBu6)
[![View nuget packages](https://img.shields.io/nuget/v/Singulink.Collections.Weak.svg)](https://www.nuget.org/packages/Singulink.Collections.Weak/)
[![Build and Test](https://github.com/Singulink/Singulink.Collections.Weak/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink.Collections.Weak/actions?query=workflow%3A%22build+and+test%22)

**Singulink.Collections.Weak** provides a set of collection classes that store weak references to values so that the gargabe collector is free to reclaim the memory they use when they aren't being referenced from anywhere else anymore. The values returned by the collections will never be `null` - if the value was garbage collected then the collection behaves as if the value was removed from the collection.

The following collections are included in the package:
- `WeakCollection`: Collection of weakly referenced values that keeps items in an undefined order.
- `WeakList`: Collection of weakly referenced values that maintains relative insertion order.
- `WeakValueDictionary`: Collection of keys and weakly referenced values.

### About Singulink

We are a small team of engineers and designers dedicated to building beautiful, functional and well-engineered software solutions. We offer very competitive rates as well as fixed-price contracts and welcome inquiries to discuss any custom development / project support needs you may have.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

## Installation

The package is available on NuGet - simply install the `Singulink.Collections.Weak` package.

**Supported Runtimes**: Anywhere .NET Standard 2.0+ is supported, including:
- .NET Core 2.0+
- .NET Framework 4.6.1+
- Mono 5.4+
- Xamarin.iOS 10.14+
- Xamarin.Android 8.0+

## Further Reading

You can get more information and view the fully documented API on the [project documentation site](https://www.singulink.com/Docs/Singulink.Collections.Weak/).
