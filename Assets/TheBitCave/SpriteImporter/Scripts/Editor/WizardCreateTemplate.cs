//----------------------------------------------
//            Sprite Importer
//       Copyright © 2015 Marco Secchi
//           http://thebitcave.com
//----------------------------------------------
using UnityEngine;
using UnityEditor;

/// <summary>
/// A wizard to create and manage import templates.
/// </summary>
namespace TheBitCave.SpriteImporter {

	public class WizardCreateTemplate : ScriptableWizard {

		string templateName = "";

		static string _assetPath;
		static TextureImporter _textureImporter;

		void OnWizardCreate() {
			string templatePath = TemplateUtils.TheTemplatesFolderPath + "/" + templateName + ".asset";
			if(AssetDatabase.LoadMainAssetAtPath(templatePath) == null) {
				if(_textureImporter != null) {
					TemplateUtils.GenerateTemplateFromTextureImporter(_textureImporter, templatePath);
				} else {
					TemplateUtils.SaveTemplate(_assetPath, templatePath);
				}
			} else {
				if(EditorUtility.DisplayDialog("Template Name Exists",
				                               "A template named " + templateName + " already exists. Do you want to overwrite it?",
				                               "Overwrite",
				                               "Cancel")) {
					if(_textureImporter != null) {
						TemplateUtils.GenerateTemplateFromTextureImporter(_textureImporter, templatePath);
					} else {
						TemplateUtils.SaveTemplate(_assetPath, templatePath, true);
					}
				}
			}
			AssetDatabase.Refresh();
		}

		void OnWizardOtherButton() {
			Close();
		}

		void OnWizardUpdate() {
			if (templateName.Length > 0)
			{
				isValid = true;
			} else {
				isValid = false;
			}
		}

		override protected bool DrawWizardGUI() {
			GUILayout.Space(10);
			templateName = EditorGUILayout.TextField("Template Name", templateName);

			if (!isValid)
			{
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("Please insert a valid name for the template.", MessageType.Warning);
			}
			GUILayout.Space(10);

			Event evt = Event.current;
			if (evt.type == EventType.keyUp && evt.keyCode == KeyCode.Return && templateName.Length > 0) {
				OnWizardCreate();
				Close();
			}

			return true;
		}

		/// <summary>
		/// Shows the wizard window
		/// </summary>
		/// <param name="assetPath">The sprite importer asset path name.</param>
		internal static void ShowWizard(string assetPath) {
			if(AssetDatabase.LoadMainAssetAtPath(assetPath) == null) {
				Debug.LogError("Asset " + assetPath + "does not exist.");
				return;
			}

			_assetPath = assetPath;
			ScriptableWizard.DisplayWizard<WizardCreateTemplate>("Create Template", "Create", "Cancel");
		}

		/// <summary>
		/// Shows the wizard window
		/// </summary>
		/// <param name="texture">The texture from where to copy the import assets.</param>
		internal static void ShowWizard(TextureImporter textureImporter) {
			if(textureImporter == null) {
				Debug.LogError("Template generating TextureImporter is null.");
				return;
			}
			_textureImporter = textureImporter;
			ScriptableWizard.DisplayWizard<WizardCreateTemplate>("Create Template from Texture", "Create", "Cancel");
		}
	}
}