 using UnityEngine;
 using UnityEditor;
 
 public class TexturePostProcessor:AssetPostprocessor{
     void OnPreprocessTexture(){
         if(assetPath.Contains("DirectoryOfInterest")){
            VideoClipImporter importer=assetImporter as VideoClipImporter;
            Debug.LogWarning("Quality vor: "+importer.quality+" auf "+assetPath);
            importer.quality=1;
         }
     }
 }