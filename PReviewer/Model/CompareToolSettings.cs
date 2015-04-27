using System;
using GalaSoft.MvvmLight;

namespace PReviewer.Model
{
    public class CompareToolSettings : ViewModelBase, IEquatable<CompareToolSettings>, IComparable<CompareToolSettings>
    {
        private string _Name;
        private string _Parameters;
        private string _ExePath;

        public static readonly string DefaultParameters = "$Base $Head";

        public CompareToolSettings()
        {
            Parameters = DefaultParameters;
        }

        public CompareToolSettings(SerializableCompareToolSettings serializableCompareToolSettings)
        {
            this.UpdateFrom(serializableCompareToolSettings);
        }

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                RaisePropertyChanged();
            }
        }

        public string Parameters
        {
            get { return _Parameters; }
            set
            {
                _Parameters = value;
                RaisePropertyChanged();
            }
        }

        public string ExePath
        {
            get { return _ExePath; }
            set
            {
                _ExePath = value;
                RaisePropertyChanged();
            }
        }

        public SerializableCompareToolSettings ToSerializable()
        {
            return new SerializableCompareToolSettings()
            {
                Name = this.Name,
                Parameters = this.Parameters,
                ExePath = this.ExePath
            };
        }

        public void UpdateFrom(SerializableCompareToolSettings serializable)
        {
            Name = serializable.Name;
            Parameters = serializable.Parameters;
            ExePath = serializable.ExePath;
        }

        public int CompareTo(CompareToolSettings other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return 0;
            }
            var lName = this.Name ?? "";
            var rName = other.Name ?? "";
            var nameCmp = string.CompareOrdinal(lName, rName);
            if (nameCmp != 0)
            {
                return nameCmp;
            }
            var lExePath = this.ExePath ?? "";
            var rExePath = other.ExePath ?? "";
            var pathCmp = string.CompareOrdinal(lExePath, rExePath);
            if (pathCmp != 0)
            {
                return pathCmp;
            }

            var lParam = this.Parameters ?? "";
            var rParam = other.Parameters ?? "";
            return string.CompareOrdinal(lParam, rParam);
        }

        public bool Equals(CompareToolSettings other)
        {
            return this.CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return (Name ?? "") + (ExePath ?? "") + (Parameters ?? "");
        }

        public bool IsValidSetting()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }
            return !string.IsNullOrWhiteSpace(ExePath);
        }
    }
}
