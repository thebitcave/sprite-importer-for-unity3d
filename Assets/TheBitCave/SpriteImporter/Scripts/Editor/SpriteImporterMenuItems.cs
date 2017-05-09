//----------------------------------------------
//            Sprite Importer
//       Copyright © 2015 Marco Secchi
//           http://thebitcave.com
//----------------------------------------------

using UnityEngine;
using UnityEditor;

namespace TheBitCave.SpriteImporter {

	public static class SpriteImporterMenuItems {

		/// <summary>
		/// This menu item creates an asset postprocessor in the selected folder.
		/// </summary>
		[MenuItem("Assets/TBC Sprite Importer/New Import Settings", false, 700)]
		private static void NewImportSettings() {
			string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			string importerDataPath = folderPath + "/" + SpriteImporterData.TheImporterDataAssetName;
			SpriteImporterData asset = (SpriteImporterData)AssetDatabase.LoadAssetAtPath(importerDataPath, typeof(SpriteImporterData));
			if(asset != null) {
				if(EditorUtility.DisplayDialog("Override Settings",
				                               "Asset " + importerDataPath + "already exists. Do you want to overwrite it?",
				                               "Cancel",
				                               "Confirm")) {
					return;
				}
			}
			CreateImporter(asset, importerDataPath);
		}

		/// <summary>
		/// Validator for <see cref="CreateImportSettings"/>
		/// </summary>
		/// <returns><c>true</c>, if selection is a folder, <c>false</c> otherwise.</returns>
		[MenuItem("Assets/TBC Sprite Importer/New Import Settings", true)]
		private static bool NewImportSettingsValidation() {
			return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
		}

		/// <summary>
		/// This menu item creates an asset postprocessor in the selected folder.
		/// </summary>
		[MenuItem("Assets/TBC Sprite Importer/New Template", false, 750)]
		private static void NewTemplate() {
			string templateDataPath = TemplateUtils.TheTemplatesFolderPath + "/" + TemplateUtils.TheTemplateAssetName;
			templateDataPath = AssetDatabase.GenerateUniqueAssetPath(templateDataPath);
			SpriteImporterData asset = (SpriteImporterData)AssetDatabase.LoadAssetAtPath(templateDataPath, typeof(SpriteImporterData));
			CreateImporter(asset, templateDataPath);
		}

		/// <summary>
		/// Copies the sprite data as a template: the menu will be enabled only if the
		/// texture is of type Sprite (2D and UI)
		/// </summary>
		[MenuItem("Assets/TBC Sprite Importer/Create Template From Sprite", false, 751)]
		private static void CreateTemplateFromSprite() {
			string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			TextureImporter textureImporter = (TextureImporter) TextureImporter.GetAtPath(assetPath);
			WizardCreateTemplate.ShowWizard(textureImporter);
		}

		[MenuItem("Assets/TBC Sprite Importer/Create Template From Sprite", true)]
		private static bool CreateTemplateFromSpriteValidation() {
			return TextureUtils.IsSpriteTexture(Selection.activeObject);
		}

		#region Utility methods

		/// <summary>
		/// Utility method to create the importer
		/// </summary>
		/// <param name="asset">The asset</param>
		/// <param name="path">The path where the asset will be added</param>
		private static void CreateImporter(SpriteImporterData asset, string path) {
			asset = ScriptableObject.CreateInstance("SpriteImporterData") as SpriteImporterData;
			asset.Init();
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			Selection.activeObject = asset;
		}

		#endregion
	}
}