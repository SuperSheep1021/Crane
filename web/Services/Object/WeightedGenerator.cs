using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// 带权重的元素模型
/// </summary>
/// <typeparam name="T">元素类型</typeparam>
public class WeightedItem<T>
{
    /// <summary>
    /// 元素值
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// 权重值（必须≥0）
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// 初始化带权重的元素
    /// </summary>
    /// <param name="value">元素值</param>
    /// <param name="weight">权重值（≥0）</param>
    /// <exception cref="ArgumentOutOfRangeException">权重为负数时抛出</exception>
    public WeightedItem(T value, double weight)
    {
        if (weight < 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "权重不能为负数");

        Value = value;
        Weight = weight;
    }
}
public class WeightedGenerator<T>
{
    private readonly List<WeightedItem<T>> _items = new List<WeightedItem<T>>();
    private readonly Random _random = new Random();

    /// <summary>
    /// 元素总数
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// 添加带权重的元素
    /// </summary>
    /// <param name="value">元素值</param>
    /// <param name="weight">权重（≥0）</param>
    public void AddItem(T value, double weight)
    {
        _items.Add(new WeightedItem<T>(value, weight));
    }

    /// <summary>
    /// 添加带权重的元素
    /// </summary>
    public void AddItem(WeightedItem<T> item)
    {
        _items.Add(item ?? throw new ArgumentNullException(nameof(item)));
    }

    /// <summary>
    /// 移除元素
    /// </summary>
    /// <param name="value">要移除的元素值</param>
    /// <returns>是否移除成功</returns>
    public bool RemoveItem(T value)
    {
        var itemToRemove = _items.FirstOrDefault(item => item.Value?.Equals(value) ?? false);
        if (itemToRemove != null)
        {
            _items.Remove(itemToRemove);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 更新元素权重
    /// </summary>
    /// <param name="value">元素值</param>
    /// <param name="newWeight">新权重（≥0）</param>
    /// <returns>是否更新成功</returns>
    public bool UpdateWeight(T value, double newWeight)
    {
        if (newWeight < 0)
            throw new ArgumentOutOfRangeException(nameof(newWeight), "权重不能为负数");

        var item = _items.FirstOrDefault(i => i.Value?.Equals(value) ?? false);
        if (item != null)
        {
            item.Weight = newWeight;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 生成单个随机结果（按权重比例）
    /// </summary>
    /// <returns>随机选中的元素</returns>
    /// <exception cref="InvalidOperationException">无元素或总权重为0时抛出</exception>
    public T Generate()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("没有可生成的元素");

        // 计算总权重
        double totalWeight = _items.Sum(item => item.Weight);
        if (totalWeight <= 0)
            throw new InvalidOperationException("总权重必须大于0");

        // 生成0~总权重之间的随机数
        double randomValue = _random.NextDouble() * totalWeight;

        // 累积权重判断选中项
        double accumulatedWeight = 0;
        foreach (var item in _items)
        {
            accumulatedWeight += item.Weight;
            if (randomValue < accumulatedWeight)
            {
                return item.Value;
            }
        }

        // 理论上不会走到这里（除非计算精度问题）
        return _items.Last().Value;
    }

    /// <summary>
    /// 生成多个随机结果
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="allowDuplicate">是否允许重复结果</param>
    /// <returns>随机结果列表</returns>
    /// <exception cref="ArgumentOutOfRangeException">生成数量为负数时抛出</exception>
    /// <exception cref="InvalidOperationException">不允许重复且数量超过元素总数时抛出</exception>
    public List<T> GenerateMultiple(int count, bool allowDuplicate = true)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "生成数量不能为负数");
        if (count == 0)
            return new List<T>();
        if (!allowDuplicate && count > _items.Count)
            throw new InvalidOperationException("不允许重复时，生成数量不能超过元素总数");

        var results = new List<T>();
        var tempItems = allowDuplicate ? _items : new List<WeightedItem<T>>(_items);

        for (int i = 0; i < count; i++)
        {
            if (!allowDuplicate && tempItems.Count == 0)
                break;

            // 生成单个结果
            double totalWeight = tempItems.Sum(item => item.Weight);
            double randomValue = _random.NextDouble() * totalWeight;
            double accumulatedWeight = 0;
            WeightedItem<T> selectedItem = null;

            foreach (var item in tempItems)
            {
                accumulatedWeight += item.Weight;
                if (randomValue < accumulatedWeight)
                {
                    selectedItem = item;
                    break;
                }
            }

            if (selectedItem != null)
            {
                results.Add(selectedItem.Value);
                if (!allowDuplicate)
                {
                    tempItems.Remove(selectedItem); // 不允许重复时移除已选中项
                }
            }
        }

        return results;
    }

    /// <summary>
    /// 清空所有元素
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

}
