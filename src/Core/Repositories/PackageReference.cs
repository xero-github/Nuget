﻿using System;

namespace NuGet
{
    public class PackageReference : IEquatable<PackageReference>
    {
        public PackageReference(string id, SemanticVersion version, IVersionSpec versionConstraint)
        {
            Id = id;
            Version = version;
            VersionConstraint = versionConstraint;
        }

        public string Id { get; private set; }
        public SemanticVersion Version { get; private set; }
        public IVersionSpec VersionConstraint { get; set; }

        public override bool Equals(object obj)
        {
            var reference = obj as PackageReference;
            if (reference != null)
            {
                return Equals(reference);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode()*3137 + Version.GetHashCode();
        }

        public override string ToString()
        {
            if (VersionConstraint == null)
            {
                return Id + " " + Version;
            }
            return Id + " " + Version + " (" + VersionConstraint + ")";
        }

        public bool Equals(PackageReference other)
        {
            return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase) &&
                   Version == other.Version;
        }
    }
}
