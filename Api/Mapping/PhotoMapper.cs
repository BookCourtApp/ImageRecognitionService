//using System.Text.Json;
//using Newtonsoft.Json.Linq;
//
//using Core.Models;
//
//namespace Api.Mapping;
//public class PhotoMapper
//{
//    public List<Photo> ProceedAllPhotos(JsonElement element)
//    {
//        try{
//            List<Photo> ArrInstance = new List<Photo>();
//            List<BookMarkup> TempBookList = new List<BookMarkup>();
//            Photo Instance;
//            if (element.ValueKind == JsonValueKind.Object){
//                foreach (JsonProperty property in element.EnumerateObject()){
//                    if (property.Value.ValueKind == JsonValueKind.Array && property.Name == "photos"){
//                        foreach (JsonElement arrayElement in property.Value.EnumerateArray()){
//                            string ImageValue = GetDataByName(arrayElement, "image");
//                            foreach (JsonProperty PhotosProperties in arrayElement.EnumerateObject()){
//                                if (PhotosProperties.Value.ValueKind == JsonValueKind.Array && PhotosProperties.Name == "bookmarkups"){
//                                    Instance = new Photo();
//                                    Instance.BookMarkups = new List<BookMarkup>();
//                                    foreach (JsonElement Book in PhotosProperties.Value.EnumerateArray()){
//                                        Instance.Image = System.Text.Encoding.ASCII.GetBytes(ImageValue);
//                                        List<Dictionary<string, string>> BookM = GetModelsFromArrayByModel(Book);
//                                        JsonElement? Texts = GetArrayByName(Book, "textmarkups");
//                                        List<Dictionary<string, string>> TextM = GetModelsFromArrayByModel((JsonElement)Texts);
//                                        List<TextMarkup> arr = new List<TextMarkup>();
//                                        for (int i = 0; i < TextM.Count; i++){
//                                            arr.Add(new TextMarkup{
//                                                x = Convert.ToInt32(TextM[i]["x"]),
//                                                y = Convert.ToInt32(TextM[i]["y"]),
//                                                Height = Convert.ToInt32(TextM[i]["height"]),
//                                                Width = Convert.ToInt32(TextM[i]["width"]),
//                                                Type = TextM[i]["type"],
//                                                Text = TextM[i]["text"]
//                                            });
//                                        }
//                                        Instance.BookMarkups.Add(
//                                            new BookMarkup{
//                                                x = Convert.ToInt32(BookM[0]["x"]),
//                                                y = Convert.ToInt32(BookM[0]["y"]),
//                                                Height = Convert.ToInt32(BookM[0]["height"]),
//                                                Width = Convert.ToInt32(BookM[0]["width"]),
//                                                TextMarkups = arr
//                                            });
//                                    }
//                                    ArrInstance.Add(Instance);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            return ArrInstance;}
//        catch (Exception ex){
//            Console.WriteLine($"345435{ex.Message}");
//            return default!;
//        }
//    }
//    public List<Dictionary<string, string>> GetModelsFromArrayByModel(JsonElement element)
//    {
//        try{
//            List<Dictionary<string, string>> ResultList = new List<Dictionary<string, string>>();
//            Dictionary<string, string> result = new Dictionary<string, string>();
//            if (element.ValueKind == JsonValueKind.Array){
//                foreach (JsonElement arrayElement in element.EnumerateArray()){
//                    if (arrayElement.ValueKind == JsonValueKind.Object){
//                        foreach (JsonProperty property in arrayElement.EnumerateObject()){
//                            string propertyName = property.Name;
//                            JsonElement propertyValue = property.Value;
//                            if (propertyValue.ValueKind == JsonValueKind.String){
//                                result.Add(propertyName, propertyValue.GetString());
//                            }
//                            if (propertyValue.ValueKind == JsonValueKind.Number){
//                                result.Add(propertyName, propertyValue.GetInt32().ToString());
//                            }
//                        }
//                        ResultList.Add(result);
//                    }
//                    result = new Dictionary<string, string>();
//                }
//            }
//            else if (element.ValueKind == JsonValueKind.Object){
//                foreach (JsonProperty property in element.EnumerateObject()){
//                    string propertyName = property.Name;
//                    JsonElement propertyValue = property.Value;
//                    if (propertyValue.ValueKind == JsonValueKind.String){
//                        result.Add(propertyName, propertyValue.GetString());
//                    }
//                    if (propertyValue.ValueKind == JsonValueKind.Number){
//                        result.Add(propertyName, propertyValue.GetInt32().ToString());
//                    }
//                }
//                ResultList.Add(result);
//            }
//            return ResultList;
//        }
//        catch (Exception ex){
//            Console.WriteLine("Something went wrong 894");
//            return default!;
//        }
//    }
//    public string GetDataByName(JsonElement element, string name)
//    {
//        try{
//            if (element.ValueKind == JsonValueKind.Object){
//                foreach (JsonProperty property in element.EnumerateObject()){
//                    string propertyName = property.Name;
//                    JsonElement propertyValue = property.Value;
//                    if (propertyName.ToLower() == name.ToLower()){
//                        return propertyValue.GetString();
//                    }
//                    //if Data field have nested json object like "Data":{"MoreData":{"":""}}
//                    if (propertyValue.ValueKind == JsonValueKind.Object){
//                        string nestedString = GetDataByName(propertyValue, name);
//                        if (nestedString != "none"){
//                            return nestedString;
//                        }
//                    }
//                    if (propertyValue.ValueKind == JsonValueKind.Array){
//                        foreach (JsonElement arrayElement in propertyValue.EnumerateArray()){
//                            Console.WriteLine("Itterating through array");
//                            string nestedString = GetDataByName(arrayElement, name);
//                            if (nestedString != "none"){
//                                return nestedString;
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        catch (Exception ex){
//            Console.WriteLine($"Something went wrong:{ex.Message}");
//        }
//        return "none";
//    }
//
//    public JsonElement? GetArrayByName(JsonElement element, string name)
//    {
//        try{
//            if (element.ValueKind == JsonValueKind.Object){
//                foreach (JsonProperty property in element.EnumerateObject()){
//                    string propertyName = property.Name;
//                    JsonElement propertyValue = property.Value;
//                    if (propertyValue.ValueKind == JsonValueKind.Array && propertyName == name){
//                        return propertyValue;
//                    }
//                    if (propertyValue.ValueKind == JsonValueKind.Object){
//                        JsonElement? nestedArray = GetArrayByName(propertyValue, name);
//                        if (nestedArray != null){
//                            return nestedArray;
//                        }
//                    }
//                    if (propertyValue.ValueKind == JsonValueKind.Array){
//                        foreach (JsonElement arrayElement in propertyValue.EnumerateArray()){
//                            JsonElement? nestedArray = GetArrayByName(arrayElement, name);
//                            if (nestedArray != null){
//                                return nestedArray;
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        catch (Exception ex){
//            Console.WriteLine($"Something went wrong:{ex.Message}");
//        }
//        return null;
//    }
//}