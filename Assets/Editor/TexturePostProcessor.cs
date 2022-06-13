 using UnityEngine;
 using UnityEditor;
 
 public class TexturePostProcessor:AssetPostprocessor{
     void OnPreprocessTexture(){
         if(assetPath.Contains("DirectoryOfInterest")){
            MovieImporter importer=assetImporter as MovieImporter;
            Debug.LogWarning("Quality vor: "+importer.quality+" auf "+assetPath);
            importer.quality=1;
         }
     }
 }