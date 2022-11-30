<div class="article">

# Singulink.Collections.Weak

## Summary

**Singulink.Collections.Weak** provides a set of collection classes that store weak references to values so that the gargabe collector is free to reclaim the memory they use when they aren't being referenced from anywhere else anymore. The values returned by the collections will never be `null` - if the value was garbage collected then the collection behaves as if the value was removed from the collection.

The following collections are included in the package:
- `WeakCollection`: Collection of weakly referenced values that keeps items in an undefined order.
- `WeakList`: Collection of weakly referenced values that maintains relative insertion order.
- `WeakValueDictionary`: Collection of keys and weakly referenced values.

**Singulink.Collections.Weak** is part of the **Singulink Libraries** collection. Visit https://github.com/Singulink/ to see the full list of libraries available.

## Usage

On .NET Core 3+ and .NET 5+, `WeakCollection` and `WeakValueDictionary` remove entries that point to garbage collected values as they are encountered, so if you regularly enumerate over all the values then additional cleaning may not be necessary. `WeakList` needs to have the `Clean()` method called periodically if you want to remove entries that point to values that no longer exist from memory.

All the collections have the following properties to help with cleaning:
- `AutoCleanAddCount`: Sets the number of `Add()` operations that automatically triggers the `Clean()` method to run.
- `AddCountSinceLastClean`: Gets the number of add operations that have been performed since the last cleaning. Can be used by your code to implement more complex custom logic for triggering a cleaning.
- `TrimExcessDuringClean`: Sets a value indicating whether to automatically call `TrimExcess()` whenever `Clean()` is called. Useful for keeping memory usage to a minimum when relying on the automatic cleaning functionality.

## Information and Links

Here are some additonal links to get you started:

- [API Documentation](api/index.md) - Browse the fully documented API here.
- [Chat on Discord](https://discord.gg/EkQhJFsBu6) - Have questions or want to discuss the library? This is the place for all Singulink project discussions.
- [Github Repo](https://github.com/Singulink/Singulink.Collections.Weak) - File issues, contribute pull requests or check out the code for yourself!

</div>