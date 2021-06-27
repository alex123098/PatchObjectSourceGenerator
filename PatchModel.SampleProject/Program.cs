using PatchModel.SampleProject;
using System;

var user = User.CreateNew();
Console.WriteLine("Before: ");
Console.WriteLine(user);

var input = new UserInput("TestUserNickName", default);

input.Patch(user);
Console.WriteLine("After the first patch:");
Console.WriteLine(user);

input = input with { NickName = default, Age = 20 };
input.Patch(user);
Console.WriteLine("After the second patch:");
Console.WriteLine(user);
