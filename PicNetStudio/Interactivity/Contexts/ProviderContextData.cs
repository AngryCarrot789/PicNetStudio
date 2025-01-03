// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of PicNetStudio.
// 
// PicNetStudio is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// PicNetStudio is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PicNetStudio. If not, see <https://www.gnu.org/licenses/>.
// 

namespace PicNetStudio.Interactivity.Contexts;

/// <summary>
/// A context data object that stores stores value provider functions instead of direct objects
/// </summary>
public class ProviderContextData : IContextData {
    private Dictionary<string, ObjectProvider>? map;

    public IEnumerable<KeyValuePair<string, object>> Entries {
        get {
            if (this.map == null)
                yield break;

            foreach (KeyValuePair<string, ObjectProvider> entry in this.map) {
                object? value = entry.Value.ProvideValue();
                if (value != null)
                    yield return new KeyValuePair<string, object>(entry.Key, value);
            }
        }
    }

    public ProviderContextData() {
    }

    public void SetValue<T>(DataKey<T> key, T? value) => this.SetValueRaw(key.Id, value);

    public void SetValueRaw(DataKey key, object value) => this.SetValueRaw(key.Id, value);

    public void SetValueRaw(string key, object? value) => this.SetProviderImpl(key, ObjectProvider.ForValue(value));

    public void SetProvider<T>(DataKey<T> key, Func<T> provider) => this.SetProviderRaw(key.Id, () => provider());

    public void SetProviderRaw(DataKey key, Func<object> provider) => this.SetProviderRaw(key.Id, provider);

    public void SetProviderRaw(string key, Func<object> provider) => this.SetProviderImpl(key, ObjectProvider.ForProvider(provider));

    private void SetProviderImpl(string key, ObjectProvider provider) {
        (this.map ??= new Dictionary<string, ObjectProvider>())[key] = provider;
    }

    public bool TryGetContext(string key, out object value) {
        if (this.map != null && this.map.TryGetValue(key, out ObjectProvider provider)) {
            return (value = provider.ProvideValue()) != null;
        }

        value = null;
        return false;
    }

    public bool ContainsKey(string key) => this.TryGetContext(key, out _);

    public IContextData Clone() {
        return new ProviderContextData() {
            map = this.map != null ? new Dictionary<string, ObjectProvider>(this.map) : null
        };
    }

    public void Merge(IContextData ctx) {
        Dictionary<string, ObjectProvider>? dictionary;
        if (ctx is ProviderContextData provider) {
            if (provider.map != null) {
                if ((dictionary = this.map) == null) {
                    this.map = new Dictionary<string, ObjectProvider>(provider.map);
                }
                else {
                    foreach (KeyValuePair<string, ObjectProvider> entry in provider.map) {
                        dictionary[entry.Key] = entry.Value;
                    }
                }
            }
        }
        else if (!(ctx is EmptyContext)) {
            using IEnumerator<KeyValuePair<string, object>> enumerator = ctx.Entries.GetEnumerator();
            if (!enumerator.MoveNext())
                return;

            dictionary = this.map ??= new Dictionary<string, ObjectProvider>();
            do {
                KeyValuePair<string, object> entry = enumerator.Current;
                dictionary[entry.Key] = ObjectProvider.ForValue(entry.Value);
            } while (enumerator.MoveNext());
        }
    }

    private struct ObjectProvider {
        private int type;
        private object? value;

        public static ObjectProvider ForValue(object? value) => new ObjectProvider() {
            type = 1, value = value
        };

        public static ObjectProvider ForProvider(Func<object> provider) => new ObjectProvider() {
            type = 2, value = provider
        };

        public object? ProvideValue() {
            switch (this.type) {
                case 1: return this.value;
                case 2: return ((Func<object>) this.value!)();
                default: throw new Exception("Invalid object provider");
            }
        }
    }
}