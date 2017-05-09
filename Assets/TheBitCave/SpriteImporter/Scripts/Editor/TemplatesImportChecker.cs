//----------------------------------------------
//            Sprite Importer
//       Copyright © 2015 Marco Secchi
//           http://thebitcave.com
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// This postprocessor checks if the template folder is changing content,
/// in order to regenerate the template menu items.
/// </summary>
namespace TheBitCave.SpriteImporter {

	public class TemplatesImportChecker : AssetPostprocessor {

		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if(AreProcessedAssetsIncludedInTemplateFolder(importedAssets) ||
			   AreProcessedAssetsIncludedInTemplateFolder(deletedAssets) ||
			   AreProcessedAssetsIncludedInTemplateFolder(movedAssets) ||
			   AreProcessedAssetsIncludedInTemplateFolder(movedFromAssetPaths)) {
				TemplateUtils.GenerateNewFromTemplatesMenuItemsScript();
				TemplateUtils.GenerateApplyTemplateToSelectionMenuItemsScript();
			}
		}

		static bool AreProcessedAssetsIncludedInTemplateFolder(string[] assets) {
			foreach(string asset in assets) {
				if(asset.StartsWith(TemplateUtils.TheTemplatesFolderPath + "/"))
					return true;
			}
			return false;
		}
	}
}
