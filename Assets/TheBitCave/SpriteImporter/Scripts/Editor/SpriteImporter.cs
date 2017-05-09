//----------------------------------------------
//            Sprite Importer
//       Copyright © 2015 Marco Secchi
//           http://thebitcave.com
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Sprite postprocessor.
/// </summary>

namespace TheBitCave.SpriteImporter {
	
	public class SpriteImporter : AssetPostprocessor {

		/// <summary>
		/// If sprite post processor scriptable object exists, imports the texture settings.
		/// </summary>
		void OnPreprocessTexture() {
			string postprocessorDataPath = GetImporterDataPath(assetPath);
			SpriteImporterData importerData = (SpriteImporterData)AssetDatabase.LoadAssetAtPath(postprocessorDataPath, typeof(SpriteImporterData));
			if(importerData != null && importerData.importerEnabled) {
				TextureUtils.ImportTexture(importerData, assetImporter as TextureImporter);
				if(assetPath.Contains(SpriteImporterData.ThePivotMapSuffix)) {
					(assetImporter as TextureImporter).isReadable = true;
				}
			}
		}

		void OnPostprocessTexture(Texture2D texture2D) {
			string postprocessorDataPath = GetImporterDataPath(assetPath);
			SpriteImporterData importerData = (SpriteImporterData)AssetDatabase.LoadAssetAtPath(postprocessorDataPath, typeof(SpriteImporterData));
			if(importerData != null && importerData.importerEnabled) {
				TextureImporter textureImporter = assetImporter as TextureImporter;
				TextureUtils.SetSlicingSettings(importerData, texture2D, textureImporter);
			}
		}

		/// <summary>
		/// Reimports moved assets
		/// </summary>
		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if(movedAssets.Length < 1)
				return;
			string postprocessorDataPath = GetImporterDataPath(movedAssets[0]);
			SpriteImporterData importerData = (SpriteImporterData)AssetDatabase.LoadAssetAtPath(postprocessorDataPath, typeof(SpriteImporterData));
			if(importerData != null && importerData.importerEnabled) {
				foreach(string path in movedAssets) {
					AssetDatabase.ImportAsset(path);
				}
			}
		}
		
		/// <summary>
		/// The postprocessor asset data path.
		/// </summary>
		/// <returns>The postprocessor data path.</returns>
		/// <param name="assetPath">The asset path from where to extract the datapath.</param>
		static string GetImporterDataPath(string path) {
			string[] ar = path.Split('/');
			List<string> list = ar.ToList();
			list.RemoveAt(list.Count - 1);
			return string.Join("/", list.ToArray()) + "/" + SpriteImporterData.TheImporterDataAssetName;
		}
	}
}
