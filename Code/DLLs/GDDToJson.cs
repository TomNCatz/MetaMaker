using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibT.Serialization;

public static class GDDToJson
{
	private class HackClass
	{
		[JsonExtensionData]
		public Dictionary<string, JsonElement>? ExtensionData { get; set; }
	}
	/// <summary>
	/// Convert a <see cref="GenericDataDictionary"/> to a JSON string
	/// </summary>
	public static string GddToJson( this GenericDataDictionary self )
	{
		return JsonSerializer.Serialize(AddToken(self), new JsonSerializerOptions { WriteIndented = true });
	}
		
	/// <summary>
	/// Convert a <see cref="GenericDataList"/> to a JSON string
	/// </summary>
	public static string GddToJson( this GenericDataList self )
	{
		return JsonSerializer.Serialize(AddToken(self), new JsonSerializerOptions { WriteIndented = true });
	}
	
	/// <summary>
	/// Convert a JSON string to a <see cref="GenericDataDictionary"/>
	/// </summary>
	public static void GddFromJson( this GenericDataDictionary self, string json )
	{
		self.values.Clear();
		
		var hack = JsonSerializer.Deserialize<HackClass>(json);
		foreach (var pair in hack.ExtensionData)
		{
			var item = ReadItem( pair.Value );
			self.values.Add( pair.Key, item );
		}
	}
	
	// /// <summary>
	// /// Convert a JSON string to a <see cref="GenericDataArray"/>
	// /// </summary>
	// public static void GddFromJson( this GenericDataList self, string json )
	// {
	// 	self.values.Clear();
	// 		
	// 	JArray jArray = JArray.Parse(json);
	// 	foreach( JToken jToken in jArray )
	// 	{
	// 		self.values.Add( ReadItem( jToken ) );
	// 	}
	// }
		
	/// <summary>
	/// Serializes an object to a JSON string
	/// </summary>
	/// <param name="self">the object being converted</param>
	/// <returns>a JSON string representing the object</returns>
	public static string BakeToJson( this IGddSaveable self )
	{
		return self.GetObjectData().ToJson();
	}
		
	/// <summary>
	/// Loads the data from a JSON string into the object
	/// </summary>
	/// <param name="self">Object with data to fill out</param>
	/// <param name="json">Data to fill the object with</param>
	public static void LoadFromJson( this IGddLoadable self, string json )
	{
		GenericDataDictionary data = new GenericDataDictionary();
		try
		{
			data.FromJson( json );
		}
		catch( Exception e )
		{
			Log.Except( e );
			throw;
		}
		self.SetObjectData( data );
	}
		
	// /// <summary>
	// /// Saves converts to a GDD and saves that GDD to a JSON file
	// /// </summary>
	// /// <param name="self">target convertible to save</param>
	// /// <param name="filePath">full path to save as</param>
	// /// <param name="overWrite">should it overwrite if exists</param>
	// /// <returns>true if successful</returns>
	// public static bool SaveToJsonFile(this IGddConvertible self, string filePath, bool overWrite = true)
	// {
	// 	return Serializer.PutTextInFile(filePath,self.BakeToJson(), overWrite);
	// }
		
	/// <summary>
	/// Loads a JSON file and attempts to deserialize it into a GDD, then sets the object's data based on that GDD
	/// </summary>
	/// <param name="self">Target object to load into</param>
	/// <param name="filePath">full path to load from</param>
	public static void LoadFromJsonFile(this IGddConvertible self, string filePath)
	{
		var json = Serializer.GetTextFromFile(filePath);
		var gdd = new GenericDataDictionary();
		gdd.FromJson(json);
		self.SetObjectData(gdd);
	}
		
	// /// <summary>
	// /// Saves converts to a GDD and saves that GDD to a JSON file
	// /// </summary>
	// /// <param name="self">target convertible to save</param>
	// /// <param name="filePath">full path to save as</param>
	// /// <param name="overWrite">should it overwrite if exists</param>
	// /// <returns>the async Task. This task will return an exception if it fails, null if it succeeds</returns>
	// public static Task<Response> SaveToJsonFileAsync(this IGddConvertible self, string filePath, bool overWrite = true)
	// {
	// 	return Serializer.PutTextInFileAsync(filePath,self.BakeToJson(), overWrite);
	// }
		
	/// <summary>
	/// Loads a JSON file and attempts to deserialize it into a GDD, then sets the object's data based on that GDD
	/// </summary>
	/// <param name="self">Target object to load into</param>
	/// <param name="filePath">full path to load from</param>
	/// <returns>will return an exception if it fails, null if it succeeds</returns>
	public static async Task<Exception> LoadFromJsonFileAsync(this IGddConvertible self, string filePath)
	{
		var response = await Serializer.GetTextFromFileAsync(filePath);
			
		if (!response.Success)
		{
			return response.Exception;
		}
			
		var gdd = new GenericDataDictionary();
		gdd.FromJson(response.Payload);
		self.SetObjectData(gdd);

		return null;
	}
		
		
	private static GenericDataObject ReadObject( JsonElement element)
	{
		var pairs = element.EnumerateObject();
			
		var self = new GenericDataDictionary();
			
		foreach (var pair in pairs)
		{
			GenericDataObject item = ReadItem( pair.Value );
			self.values.Add( pair.Name, item );
		}

		return self;
	}

	private static GenericDataList ReadArray(JsonElement element)
	{
		var values = element.EnumerateArray();

		GenericDataList self = new GenericDataList();
			
		foreach( var item in values )
		{
			self.values.Add( ReadItem( item ) );
		}
		return self;
	}
		
	private static GenericDataObject ReadItem(JsonElement element)
	{
		switch(element.ValueKind)
		{
			// case JTokenType.Object : 
			// {
			// 	JObject jObject = JObject.Parse(token.ToString());
			// 	return ReadObject( jObject );
			// }
			// case JTokenType.Array : return ReadArray(token);
			// case JTokenType.Integer : return GenericDataObject.CreateGdo( token.ToObject<int>() );
			// case JTokenType.Float : return GenericDataObject.CreateGdo( token.ToObject<float>() );
			// case JTokenType.String : return GenericDataObject.CreateGdo( token.ToObject<string>() );
			// case JTokenType.Boolean : return GenericDataObject.CreateGdo(  token.ToObject<bool>() );
			// case JTokenType.Date : return GenericDataObject.CreateGdo( token.ToObject<DateTime>().DateTimeToString() );
			// case JTokenType.Bytes : break;
			// case JTokenType.TimeSpan : break;
			// case JTokenType.Null : return new GenericDataNull();
			// default : throw new ArgumentOutOfRangeException();
			case JsonValueKind.Undefined: break;
			case JsonValueKind.Object: return ReadObject(element);
			case JsonValueKind.Array: return ReadArray(element);
			case JsonValueKind.String: return GenericDataObject.CreateGdo(element.GetString());
			case JsonValueKind.Number:
				if (element.TryGetInt32(out int intValue)) return GenericDataObject.CreateGdo(intValue);
				element.TryGetSingle(out float floatValue); 
				return GenericDataObject.CreateGdo(floatValue);
			case JsonValueKind.True: return GenericDataObject.CreateGdo(true);
			case JsonValueKind.False: return GenericDataObject.CreateGdo(false);
			case JsonValueKind.Null: return new GenericDataNull();
			default: throw new ArgumentOutOfRangeException();
		}

		return null;
	}

	private static object AddToken(GenericDataObject dataObject)
	{
		switch(dataObject.type)
		{
			case DataTypeIndicator.OBJECT : return AddObject( dataObject );
			case DataTypeIndicator.LIST : return AddList( dataObject );
			case DataTypeIndicator.INT :
				dataObject.FromGdo(out int valInt);
				return valInt;
			case DataTypeIndicator.FLOAT : 
				dataObject.FromGdo(out float valFloat);
				return valFloat;
			case DataTypeIndicator.BOOL : 
				dataObject.FromGdo(out bool valBool);
				return valBool; 
			case DataTypeIndicator.STRING : 
				dataObject.FromGdo(out string valString);
				return valString;
			case DataTypeIndicator.NULL :
				return null;// todo this doesn't work anymore
			case DataTypeIndicator.SKIP :
				return null;
			default : throw new ArgumentOutOfRangeException();
		}
	}

	private static object AddObject(GenericDataObject dataObject)
	{
		var objectData = dataObject as GenericDataDictionary;
		if( objectData == null ) return null;

		Dictionary<string, object> dataSet = new Dictionary<string, object>();
			
		foreach( var item in objectData.values )
		{
			var token = AddToken(item.Value);
			if(token!= null) dataSet.Add( item.Key, token );
		}

		return dataSet;
	}

	private static object AddList(GenericDataObject dataObject)
	{
		var dataArray = dataObject as GenericDataList;
		if( dataArray == null ) return null;

		object[] dataSet = new object[dataArray.values.Count];

		for (int i = 0; i < dataArray.values.Count; i++)
		{
			var token = AddToken(dataArray.values[i]);
			if(token!= null) dataSet[i] = token;
		}

		return dataSet;
	}
}