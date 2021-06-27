using System;

namespace PatchModel.SampleProject
{
    public sealed class User
    {
        public Guid Id { get; }

        public string NickName { get; set; } = string.Empty;

        public int Age { get; set; }

        public User(Guid id) => Id = id;

        public static User CreateNew() => new(Guid.NewGuid());

        public override string ToString()
            => $"{{Id: {Id} NickName: {NickName} Age: {Age}}}";
    }
}
