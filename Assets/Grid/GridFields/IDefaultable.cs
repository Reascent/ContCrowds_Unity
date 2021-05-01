using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GridFields
{
  /// <summary>
  /// Implement methods for setting an object to its default state.
  /// </summary>
  public interface IDefaultable<T>
  {
    T DefaultValue { get; }

    void ResetToDefault();
    void SetDefaultValue(T t);
  }
}
