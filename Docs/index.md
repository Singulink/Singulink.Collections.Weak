# Singulink.Collections.Weak

**Singulink.Collections.Weak** provides a set of collection classes that store weak references to values so that the gargabe collector is free to reclaim the memory they use when they aren't being references from anywhere else anymore. The values returned by the collections will never be `null` - if the value was garbage collected then the collection behaves as if the value was removed from the collection.

The following collections are included in the package:
- `WeakCollection`: Collection of weakly referenced values that keeps items in an undefined order.
- `WeakList`: Collection of weakly referenced values that maintains relative insertion order.
- `WeakValueDictionary`: Collection of keys and weakly referenced values.

**Singulink.Collections.Weak** is part of the **Singulink Libraries** collection. Visit https://github.com/Singulink/ to see the full list of libraries available.

## Information and Links

Here are some additonal links to get you started:

- [API Documentation](api/index.md) - Browse the fully documented API here.
- [Chat on Gitter](https://gitter.im/Singulink/community) - Have questions or want to discuss the library? This is the place for all Singulink project discussions.
- [Github Repo](https://github.com/Singulink/Singulink.Collections.Weak) - File issues, contribute pull requests or check out the code for yourself!