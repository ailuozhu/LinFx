﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Text.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace LinFx.Utils
{
    public static partial class JsonUtils
    {
        public static object ToObject(byte[] value, bool camelCase = false, bool indented = false)
        {
            return ToObject<object>(value, camelCase, indented);
        }

        public static T ToObject<T>(byte[] value, bool camelCase = false, bool indented = false)
        {
            var s = Encoding.UTF8.GetString(value);
            return ToObject<T>(s, camelCase, indented);
        }

        public static byte[] ToBytes(object value, bool camelCase = false, bool indented = false)
        {
            var s = ToJsonString(value, camelCase, indented);
            return Encoding.UTF8.GetBytes(s);
        }

        public static string ToJson(object value, bool camelCase = false, bool indented = false)
        {
            return ToJsonString(value, camelCase, indented);
        }

        public static string ToJsonString(object value, bool camelCase = true, bool indented = false)
        {
            var options = new JsonSerializerSettings();

            if (camelCase)
                options.ContractResolver = new CamelCasePropertyNamesContractResolver();

            if (indented)
                options.Formatting = Formatting.Indented;

            //DateTimeFormat
            //options.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            options.DateFormatString = "yyyy-MM-dd HH:mm:ss";

            return JsonConvert.SerializeObject(value, options);
        }

        public static T ToObject<T>(string value, bool camelCase = false, bool indented = false)
        {
            var options = new JsonSerializerSettings();

            if (camelCase)
                options.ContractResolver = new CamelCasePropertyNamesContractResolver();

            if (indented)
                options.Formatting = Formatting.Indented;

            //options.Converters.Insert(0, new DateTimeConverter());

            return JsonConvert.DeserializeObject<T>(value, options);
        }

        private const char TypeSeperator = '|';

        /// <summary>
        /// Serializes an object with a type information included.
        /// So, it can be deserialized using <see cref="DeserializeWithType"/> method later.
        /// </summary>
        public static string SerializeWithType(object obj)
        {
            return SerializeWithType(obj, obj.GetType());
        }

        /// <summary>
        /// Serializes an object with a type information included.
        /// So, it can be deserialized using <see cref="DeserializeWithType"/> method later.
        /// </summary>
        public static string SerializeWithType(object obj, Type type)
        {
            var serialized = obj.ToJson();

            return string.Format(
                "{0}{1}{2}",
                type.AssemblyQualifiedName,
                TypeSeperator,
                serialized);
        }

        /// <summary>
        /// Deserializes an object serialized with <see cref="SerializeWithType(object)"/> methods.
        /// </summary>
        public static T DeserializeWithType<T>(string serializedObj)
        {
            return (T)DeserializeWithType(serializedObj);
        }

        /// <summary>
        /// Deserializes an object serialized with <see cref="SerializeWithType(object)"/> methods.
        /// </summary>
        public static object DeserializeWithType(string serializedObj)
        {
            var typeSeperatorIndex = serializedObj.IndexOf(TypeSeperator);
            var type = Type.GetType(serializedObj.Substring(0, typeSeperatorIndex));
            var serialized = serializedObj.Substring(typeSeperatorIndex + 1);

            var options = new JsonSerializerSettings();
            //options.Converters.Insert(0, new AbpDateTimeConverter());

            return JsonConvert.DeserializeObject(serialized, type, options);
        }

        public static object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }
    }

    /// <summary>
    /// 转化小写
    /// </summary>
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

    public class UnderlineSplitContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < propertyName.Length; i++)
            {
                var ch = propertyName[i];
                if (i == 0)
                {
                    builder.Append(char.ToLower(ch));
                }
                else
                {
                    var prev = propertyName[i - 1];
                    if (prev == '_')
                        builder.Append(char.ToLower(ch));
                    else
                        builder.Append(ch);
                }
            }
            return builder.ToString();
        }
    }

    /// <summary>  
    /// Newtonsoft.Json序列化扩展特性  
    /// <para>DateTime序列化（输出为时间戳）</para>  
    /// </summary>  
    public class TimestampConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ConvertIntDateTime(int.Parse(reader.Value.ToString()));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ConvertDateTimeInt((DateTime)value));
        }

        public static DateTime ConvertIntDateTime(int aSeconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(aSeconds);
        }

        public static int ConvertDateTimeInt(DateTime aDT)
        {
            return (int)(aDT - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }

    /// <summary>  
    /// Newtonsoft.Json序列化扩展特性  
    /// <para>String Unicode 序列化（输出为Unicode编码字符）</para>  
    /// </summary>  
    public class UnicodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToUnicode(value.ToString()));
        }

        public static string ToUnicode(string str)
        {
            byte[] bts = Encoding.Unicode.GetBytes(str);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2)
            {
                r += "\\u" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');
            }
            return r;
        }
    }

    public static class JsonUtilsExtensions
    {
        /// <summary>
        /// Converts given object to JSON string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camelCase"></param>
        /// <param name="indented"></param>
        /// <returns></returns>
        public static string ToJson(this object value, bool camelCase = true, bool indented = false)
        {
            return JsonUtils.ToJson(value, camelCase, indented);
        }

        public static byte[] ToBytes(this object value, bool camelCase = false, bool indented = false)
        {
            return JsonUtils.ToBytes(value, camelCase, indented);
        }

        public static T ToObject<T>(this string value, bool camelCase = false, bool indented = false)
        {
            return JsonUtils.ToObject<T>(value, camelCase, indented);
        }
    }

    ///// <summary>
    ///// 格式化DateTime
    ///// </summary>
    //public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    //{
    //    /// <summary>
    //    /// 获取日期格式
    //    /// </summary>
    //    public string DateTimeFormat { get; }

    //    /// <summary>
    //    /// ctor
    //    /// </summary>      
    //    /// <param name="dateTimeFormat"></param>
    //    public DateTimeOffsetJsonConverter(string dateTimeFormat)
    //    {
    //        this.DateTimeFormat = dateTimeFormat;
    //    }

    //    /// <summary>
    //    /// 读取
    //    /// </summary>
    //    /// <param name="reader"></param>
    //    /// <param name="typeToConvert"></param>
    //    /// <param name="options"></param>
    //    /// <returns></returns>
    //    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        return DateTimeOffset.Parse(reader.GetString());
    //    }

    //    /// <summary>
    //    /// 写入
    //    /// </summary>
    //    /// <param name="writer"></param>
    //    /// <param name="value"></param>
    //    /// <param name="options"></param>
    //    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    //    {
    //        value = value.ToLocalTime();
    //        writer.WriteStringValue(value.ToString(this.DateTimeFormat, CultureInfo.InvariantCulture));
    //    }
    //}

    ///// <summary>
    ///// 格式化DateTime
    ///// </summary>
    //public class DateTimeJsonConverter : JsonConverter<DateTime>
    //{
    //    /// <summary>
    //    /// 获取日期格式
    //    /// </summary>
    //    public string DateTimeFormat { get; }

    //    /// <summary>
    //    /// ctor
    //    /// </summary>      
    //    /// <param name="dateTimeFormat"></param>
    //    public DateTimeJsonConverter(string dateTimeFormat)
    //    {
    //        this.DateTimeFormat = dateTimeFormat;
    //    }

    //    /// <summary>
    //    /// 读取
    //    /// </summary>
    //    /// <param name="reader"></param>
    //    /// <param name="typeToConvert"></param>
    //    /// <param name="options"></param>
    //    /// <returns></returns>
    //    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        return DateTime.Parse(reader.GetString());
    //    }

    //    /// <summary>
    //    /// 写入
    //    /// </summary>
    //    /// <param name="writer"></param>
    //    /// <param name="value"></param>
    //    /// <param name="options"></param>
    //    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    //    {
    //        if (value.Kind == DateTimeKind.Utc)
    //            value = value.ToLocalTime();

    //        writer.WriteStringValue(value.ToString(this.DateTimeFormat, CultureInfo.InvariantCulture));
    //    }
    //}

    //public static partial class JsonUtils
    //{
    //    public static string Serialize<TValue>(TValue value, bool camelCase = true, bool indented = false, bool ignoreNullValues = false, bool ignoreReadOnlyProperties = true)
    //    {
    //        var options = new JsonSerializerOptions
    //        {
    //            WriteIndented = indented,                               //格式化json字符串
    //            AllowTrailingCommas = true,                             //可以结尾有逗号
    //            PropertyNameCaseInsensitive = true,                     //忽略大小写
    //            IgnoreNullValues = ignoreNullValues,                    //可以有空值,转换json去除空值属性
    //            IgnoreReadOnlyProperties = ignoreReadOnlyProperties     //忽略只读属性
    //        };

    //        if (camelCase)
    //            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    //        options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm"));
    //        options.Converters.Add(new DateTimeOffsetJsonConverter("yyyy-MM-dd HH:mm"));

    //        return JsonSerializer.Serialize(value, options);
    //    }

    //    public static TValue Deserialize<TValue>(string json)
    //    {
    //        return JsonSerializer.Deserialize<TValue>(json);
    //    }
    //}
}
