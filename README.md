# PatchObjectSourceGenerator
Simple source generator that adds `Patch` method to a class/struct/record that allows to update an object writable proprties with values set in properties with the same names from object of another type.
A typical use-case is partial updates.

## Example
1. Let's say we have a class that describes some entity from our domain:
```csharp
public sealed class Customer
{
    public Guid Id { get; }

    public string NickName { get; set; } = string.Empty;

    public int Age { get; set; }

    public Customer(Guid id) => Id = id;

    public override string ToString()
        => $"{{Id: {Id} NickName: {NickName} Age: {Age}}}";
}
```
and a type that represents a user input that aims to update `NickName` and/or `Age` of a Customer property:
```csharp
public sealed record CustomerInput(string? NickName, int? Age);
```

2. To use `CustomerInput` as a source of partial updates for `Customer` you need to modify the definition of `CustomerInput` as follows:
```csharp
using PatchModel.Attributes;

[PatchesType(typeof(Customer))]
public sealed partial record CustomerInput(string? NickName, int? Age);
```

3. Then just call `Patch` instance method of `CustomerInput` when you want to update state of a `Customer` instance:
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateCustomer([FromRoute] Guid id, [FromBody] CustomerInput input)
{
    // ... load your customer
    input.Patch(customer);
    // ... save changes and return result
}
