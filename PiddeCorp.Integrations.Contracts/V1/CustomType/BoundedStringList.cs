using System.Collections;

namespace PiddeCorp.Integrations.Contracts.V1.CustomType;

public class BoundedStringList : IEnumerable<string> {
    private const int MaxItemLength = 500;
    private readonly List<string> _items = [];
    private readonly bool _allowTruncation;
    
    public BoundedStringList(bool allowTruncation = false) {
        _allowTruncation = allowTruncation;
    }
    
    public void Add(string item) {
        if(item?.Length > MaxItemLength) {
            if(_allowTruncation) {
                _items.Add(item[..MaxItemLength]);
            } else {
                throw new ArgumentException($"Item length must be less than or equal to {MaxItemLength}.");
            }
        } else {
            _items.Add(item ?? string.Empty);
        }
    }
    
    public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
}
