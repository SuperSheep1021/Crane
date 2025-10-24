using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Text;

public class GameConfig : LCObject
{
    Dictionary<string, object> m_data = new Dictionary<string, object>();
    public GameConfig(string className) : base(className)
    {

    }

}
