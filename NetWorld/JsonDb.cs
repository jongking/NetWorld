using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace NetWorld
{
    public class JsonDb
    {
        private Dictionary<string, string> _dict = new Dictionary<string, string>();
        private readonly string _dbfile;
        static readonly JavaScriptSerializer Jss = new JavaScriptSerializer();

        public JsonDb(string filename)
        {
            _dbfile = filename;
            if (File.Exists(_dbfile))
            {
                var dicjson = File.ReadAllText(_dbfile);

                _dict = JsonDeserialize<Dictionary<string, string>>(dicjson);
                if (_dict == null)
                {
                    _dict = new Dictionary<string, string>();
                    Commit();
                }
            }
            else
            {
                Commit();
            }
        }

        public bool Exists(string key)
        {
            if (_dict.ContainsKey(key))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 每一次Select都应该先调用Exists来看一下key是否存在
        /// </summary>
        public T Select<T>(string key)
        {
            if (_dict.ContainsKey(key))
            {
                return JsonDeserialize<T>(_dict[key]);
            }
            throw new NullReferenceException(key);
        }

        public void Insert(string key, object value)
        {
            Updata(key, value);
        }

        public void Updata(string key, object value)
        {
            if (_dict.ContainsKey(key))
            {
                _dict[key] = JsonSerialize(value);
            }
            else
            {
                _dict.Add(key, JsonSerialize(value));
            }
            Commit();
        }

        public void Delete(string key)
        {
            if (_dict.ContainsKey(key))
            {
                _dict.Remove(key);
            }
            Commit();
        }

        public void Commit()
        {
            File.WriteAllText(_dbfile, JsonSerialize(_dict));
        }

        public void Commit(Dictionary<string, string> dictionary)
        {
            _dict = dictionary;
            Commit();
        }

        public Dictionary<string, string> GetDic()
        {
            return _dict;
        }

        private T JsonDeserialize<T>(string json)
        {
            return Jss.Deserialize<T>(json);
        }
        private string JsonSerialize(object obj)
        {
            return Jss.Serialize(obj);
        }
    }
}