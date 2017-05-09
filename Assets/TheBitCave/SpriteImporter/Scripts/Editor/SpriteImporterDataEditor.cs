//----------------------------------------------
//            Sprite Importer
//       Copyright © 2015 Marco Secchi
//           http://thebitcave.com
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Sprite post processor data editor.
/// </summary>

namespace TheBitCave.SpriteImporter {

	[CustomEditor(typeof(SpriteImporterData))]
	public class SpriteImporterDataEditor : Editor {

		// Used to check if some value has been edited.
		bool _hasChanged = false;

		int _selectedToolbarIndex;
		int _templateIndex = 0;
		string _selectedPath;
		string _folderPath;

		SpriteImporterData _data;
		SpriteImporterData _oldData;

		void OnEnable() {
			_selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			_data = target as SpriteImporterData;
			_oldData = CreateImporterDataCopy(_data);
		}

		void OnDisable() {
			if(_hasChanged) {
				if(EditorUtility.DisplayDialog("Unapplied Import Settings",
				                               "Unapplied import settings for " + _selectedPath,
				                               "Revert",
				                               "Apply")) {
					Revert();
				} else {
					Reimport();
				}
			}
		}

		/// <summary>
		/// Creates the scriptable object layout.
		/// </summary>
		public override void OnInspectorGUI ()
		{
			string importerDataName = "/" + SpriteImporterData.TheImporterDataAssetName;
			_folderPath = AssetDatabase.GetAssetPath(_data).Replace(importerDataName, "");

			// Enables/Disables the folder postprocessing
			_data.importerEnabled = GUILayout.Toggle(_data.importerEnabled, "Enable Importer");

			#region Main Elements

			EditorGUILayout.Space();

			if(!_data.importerEnabled) {
				EditorGUILayout.HelpBox("Import postprocessing disabled. Textures included in folder " + _folderPath + " can be single-edited.", MessageType.Info);
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("Enabling this panel, will automatically reset all texture settings in folder " + _folderPath + ".", MessageType.Warning);
				return;
			} else {
				if(_data.importerEnabled != _oldData.importerEnabled)
					_data.ReimportFolder();
			}

			EditorGUILayout.HelpBox("Textures included in folder " + _folderPath + " cannot be single-edited unless you disable this panel (i.e.: clicking the 'Apply' button on a texture settings will import the following data).", MessageType.Info);

			EditorGUILayout.Space();

			if(!IsTemplate(_data.name)) {
				EditorGUILayout.LabelField("Main Settings", TitleGUIStyle);
			} else {
				EditorGUILayout.LabelField("Template Settings", TitleGUIStyle);
			}

			EditorGUILayout.Space();

			int spriteModeIndex = ArrayUtility.IndexOf(SpriteImporterData.spriteModes, _data.spriteMode);
			spriteModeIndex = EditorGUILayout.Popup("Sprite Mode", spriteModeIndex, SpriteImporterData.spriteModeLabels);
			_data.spriteMode = SpriteImporterData.spriteModes[spriteModeIndex];
			if(_oldData.spriteMode != _data.spriteMode)
				_hasChanged = true;

			_data.packingTag = EditorGUILayout.TextField("    Packing Tag", _data.packingTag);
			if(_oldData.packingTag != _data.packingTag)
				_hasChanged = true;

			_data.pixelsPerUnit = EditorGUILayout.FloatField("    Pixels Per Unit", _data.pixelsPerUnit);
			if(_oldData.pixelsPerUnit != _data.pixelsPerUnit)
				_hasChanged = true;

			if(_data.spriteMode == 1) {
				InsertPivotBlock();
			}
			EditorGUILayout.Space();

			_data.generateMipMaps = EditorGUILayout.Toggle("    Generate Mip Maps", _data.generateMipMaps);
			if(_oldData.generateMipMaps != _data.generateMipMaps)
				_hasChanged = true;

			EditorGUILayout.Space();

			int filterModeIndex = ArrayUtility.IndexOf(_data.FilterModes, _data.filterMode);
			filterModeIndex = EditorGUILayout.Popup("Filter Mode", filterModeIndex, SpriteImporterData.filterModeLabels);
			_data.filterMode = _data.FilterModes[filterModeIndex];
			if(_oldData.filterMode != _data.filterMode)
				_hasChanged = true;

			EditorGUILayout.Space();

			#endregion

			#region Max Size and Format

			_selectedToolbarIndex = GUILayout.Toolbar(_selectedToolbarIndex, SpriteImporterData.platformLabels, ToolbarGUIStyle);

			GUILayout.BeginVertical(BackgroundGUIStyle);

			if(_selectedToolbarIndex == 0) {
				int maxSizesIndex = ArrayUtility.IndexOf(SpriteImporterData.maxSizeLabels, _data.maxSize.ToString());
				maxSizesIndex = EditorGUILayout.Popup("Max Size", maxSizesIndex, SpriteImporterData.maxSizeLabels);
				_data.maxSize = int.Parse(SpriteImporterData.maxSizeLabels[maxSizesIndex]);
				if(_oldData.maxSize != _data.maxSize)
					_hasChanged = true;

				int textureImporterFormatIndex = ArrayUtility.IndexOf(_data.TextureImporterFormats, _data.textureImporterFormat);
				textureImporterFormatIndex = EditorGUILayout.Popup("Format", textureImporterFormatIndex, SpriteImporterData.textureImporterFormatLabels);
				_data.textureImporterFormat = _data.TextureImporterFormats[textureImporterFormatIndex];
				if(_oldData.textureImporterFormat != _data.textureImporterFormat)
					_hasChanged = true;

				if(_data.textureImporterFormat == TextureImporterFormat.AutomaticCrunched) {
					_data.compressionQuality = EditorGUILayout.IntSlider("Compression Quality", _data.compressionQuality, 0, 100);
					if(_oldData.compressionQuality != _data.compressionQuality)
						_hasChanged = true;
				}
			} else {
				InsertPlatformPanelBlock(_selectedToolbarIndex);
			}

			GUILayout.EndVertical();

			GUILayout.Space(15);

			#endregion

			if(_data.spriteMode == 2) {
				InsertMultipleSpriteModePanelBlock();
			}

			#region Footer Buttons

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();

			GUI.enabled = _hasChanged;
			if(GUILayout.Button("Revert")) {
				Revert();
			}
			GUI.enabled = true;

			if(!IsTemplate(_data.name)) {
				if(GUILayout.Button("Apply")) {
					Reimport();
				}
			} else {
				if(GUILayout.Button("Save")) {
					Reimport();
				}
			}

			EditorGUILayout.EndHorizontal();

			#endregion

			GUILayout.Space(15);

			#region Templates

			if(!IsTemplate(_data.name)) {
				EditorGUILayout.LabelField("Templates", TitleGUIStyle);

				EditorGUILayout.Space();

				GUI.enabled = TemplateUtils.TemplateList.Length > 0;
				_templateIndex = EditorGUILayout.Popup("Saved Templates", _templateIndex, TemplateUtils.TemplateList);
				GUI.enabled = true;

				EditorGUILayout.BeginHorizontal();

				GUILayout.FlexibleSpace();

				GUI.enabled = TemplateUtils.TemplateList.Length > 0;
				if(GUILayout.Button("Apply Selected Template")) {
					_hasChanged = false;
					TemplateUtils.ApplyTemplate(TemplateUtils.TemplateList[_templateIndex], _folderPath);
				}
				GUI.enabled = true;

				if(GUILayout.Button("+")) {
					WizardCreateTemplate.ShowWizard(_selectedPath);
				}

				EditorGUILayout.EndHorizontal();

			}

			#endregion

			if(_data != null)
				EditorUtility.SetDirty(_data);
		}

		/// <summary>
		/// Checks if the importer is a template.
		/// </summary>
		bool IsTemplate(string name) {
			return name != SpriteImporterData.TheImporterDataName;
		}

		/// <summary>
		/// Inserts the mutliple sprite mode settings panel.
		/// </summary>
		void InsertMultipleSpriteModePanelBlock() {

			EditorGUILayout.LabelField("Multiple Sprite Mode Settings", TitleGUIStyle);

			EditorGUILayout.Space();

			_data.overrideSpriteEditorSettings = GUILayout.Toggle(_data.overrideSpriteEditorSettings, "Override Sprite Editor Settings");

			EditorGUILayout.Space();

			if(!_data.overrideSpriteEditorSettings) {
				EditorGUILayout.HelpBox("Multiple Sprite editing disabled. Textures included in folder " + _folderPath + " can be modified using the regular Sprite Editor.", MessageType.Info);
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("Enabling this panel, will automatically reset all texture settings in folder " + _folderPath + ".", MessageType.Warning);
			} else {
				EditorGUILayout.HelpBox("Multiple Sprite editing enabled. Textures included in folder " + _folderPath + " cannot be modified with the Sprite Editor (i.e.: changes will not apply and will revert to these settings).", MessageType.Info);

				int sliceTypeIndex = ArrayUtility.IndexOf(_data.SliceTypes, _data.sliceType);
				sliceTypeIndex = EditorGUILayout.Popup("Slice Type", sliceTypeIndex, SpriteImporterData.sliceTypeLabels);
				_data.sliceType = _data.SliceTypes[sliceTypeIndex];
				if(_oldData.sliceType != _data.sliceType)
					_hasChanged = true;

				EditorGUILayout.Space();

				if(_data.sliceType == SliceType.GridByCellSize) {
					InsertGridByCellSizeBlock();
				} else if (_data.sliceType == SliceType.GridByCellCount) {
					InsertGridByCellCountBlock();
				}

				InsertPivotBlock();

				EditorGUILayout.Space();

				_data.includeBlankSlices = EditorGUILayout.Toggle("Include Blank Slices", _data.includeBlankSlices);
				if(_oldData.includeBlankSlices != _data.includeBlankSlices)
					_hasChanged = true;

			}
		}

		/// <summary>
		/// Inserts the grid by cell size block.
		/// </summary>
		void InsertGridByCellSizeBlock() {
			_data.sliceWidth = EditorGUILayout.IntField("Slice Width", _data.sliceWidth);
			if(_oldData.sliceWidth != _data.sliceWidth)
				_hasChanged = true;

			_data.sliceHeight = EditorGUILayout.IntField("Slice Height", _data.sliceHeight);
			if(_oldData.sliceHeight != _data.sliceHeight)
				_hasChanged = true;

			InsertBorderBlock();

			EditorGUILayout.Space();

			InsertOffsetBlock();
			InsertPaddingBlock();

			EditorGUILayout.Space();
		}

		/// <summary>
		/// Inserts the grid by cell count block.
		/// </summary>
		void InsertGridByCellCountBlock() {
			_data.sliceCols = EditorGUILayout.IntField("Columns", _data.sliceCols);
			if(_oldData.sliceCols != _data.sliceCols)
				_hasChanged = true;

			_data.sliceRows = EditorGUILayout.IntField("Rows", _data.sliceRows);
			if(_oldData.sliceRows != _data.sliceRows)
				_hasChanged = true;

			InsertBorderBlock();

			EditorGUILayout.Space();

			InsertOffsetBlock();
			InsertPaddingBlock();

			EditorGUILayout.Space();
		}

		/// <summary>
		/// Inserts the border layout block.
		/// </summary>
		void InsertBorderBlock() {
			_data.sliceBorder = EditorGUILayout.Vector4Field("Slice Border", _data.sliceBorder);
			if(_oldData.sliceBorder != _data.sliceBorder)
				_hasChanged = true;
		}

		/// <summary>
		/// Inserts the offset layout block.
		/// </summary>
		void InsertOffsetBlock() {
			if(_data.offset.x < 0)
				_data.offset.x = 0;
			if(_data.offset.y < 0)
				_data.offset.y = 0;
			_data.offset = EditorGUILayout.Vector2Field("Offset", _data.offset);
			if(_oldData.offset != _data.offset)
				_hasChanged = true;

		}

		/// <summary>
		/// Inserts the padding layout block.
		/// </summary>
		void InsertPaddingBlock() {
			if(_data.padding.x < 0)
				_data.padding.x = 0;
			if(_data.padding.y < 0)
				_data.padding.y = 0;
			_data.padding = EditorGUILayout.Vector2Field("Padding", _data.padding);
			if(_oldData.padding != _data.padding)
				_hasChanged = true;
		}


		/// <summary>
		/// Utility method for inserting the pivot asset data
		/// </summary>
		void InsertPivotBlock() {
			int pivotPointIndex = _data.spriteAlignment;
			pivotPointIndex = EditorGUILayout.Popup("Pivot", pivotPointIndex, SpriteImporterData.pivotPointLabels);
			_data.spriteAlignment = pivotPointIndex;
			if(_oldData.spriteAlignment != _data.spriteAlignment)
				_hasChanged = true;

			if(pivotPointIndex == SpriteImporterData.pivotPointLabels.Length - 1) {
				_data.spritePivot = EditorGUILayout.Vector2Field(" ", _data.spritePivot);

				if(_oldData.spritePivot.x != _data.spritePivot.x && _oldData.spritePivot.y != _data.spritePivot.y)
					_hasChanged = true;
			}

			_data.pivotMapEnabled = EditorGUILayout.Toggle("Enable Pivot Map", _data.pivotMapEnabled);
			if(_oldData.pivotMapEnabled != _data.pivotMapEnabled)
				_hasChanged = true;
			if(_data.pivotMapEnabled) {
				EditorGUILayout.HelpBox("Textures with suffix " + SpriteImporterData.ThePivotMapSuffix + " will be processed as pivot maps.", MessageType.Info);
				_data.pivotMapColor = EditorGUILayout.ColorField("Pivot Color", _data.pivotMapColor);
				if(_oldData.pivotMapColor != _data.pivotMapColor)
					_hasChanged = true;
			}

		}

		/// <summary>
		/// Shows the settings for a specific platform
		/// </summary>
		/// <param name="index">The platform index to be shown.</param>
		void InsertPlatformPanelBlock(int index) {
			int i = index - 1;
			_data.platformImportDataAr[i].overrideForTargetPlatform = GUILayout.Toggle(_data.platformImportDataAr[i].overrideForTargetPlatform,
			                                                                            "Override for " + SpriteImporterData.platformLabels[index]);

			if(_oldData.platformImportDataAr[i].overrideForTargetPlatform != _data.platformImportDataAr[i].overrideForTargetPlatform)
				_hasChanged = true;

			GUI.enabled = _data.platformImportDataAr[i].overrideForTargetPlatform;

			int maxSizesIndex = ArrayUtility.IndexOf(SpriteImporterData.maxSizeLabels, _data.platformImportDataAr[i].maxSize.ToString());
			maxSizesIndex = EditorGUILayout.Popup("Max Size", maxSizesIndex, SpriteImporterData.maxSizeLabels);
			_data.platformImportDataAr[i].maxSize = int.Parse(SpriteImporterData.maxSizeLabels[maxSizesIndex]);
			if(_oldData.platformImportDataAr[i].maxSize != _data.platformImportDataAr[i].maxSize)
				_hasChanged = true;

			int textureImporterFormatIndex = ArrayUtility.IndexOf(_data.TextureImporterFormats, _data.platformImportDataAr[i].textureImporterFormat);
			textureImporterFormatIndex = EditorGUILayout.Popup("Format", textureImporterFormatIndex, SpriteImporterData.textureImporterFormatLabels);
			_data.platformImportDataAr[i].textureImporterFormat = _data.TextureImporterFormats[textureImporterFormatIndex];
			if(_oldData.platformImportDataAr[i].textureImporterFormat != _data.platformImportDataAr[i].textureImporterFormat)
				_hasChanged = true;

			if(_data.platformImportDataAr[i].textureImporterFormat == TextureImporterFormat.AutomaticCrunched) {
				if(i == (int)PlatformLabel.Tizen || i == (int)PlatformLabel.SamsungTV) {
					EditorGUILayout.HelpBox("Crunched is not supported on this platform. Falling back to 'Compressed'.", MessageType.Warning);
				} else if(i == (int)PlatformLabel.iPhone || i == (int)PlatformLabel.Android) {
					InsertPlatformCompressionQualityBlock(i);
					EditorGUILayout.HelpBox("Crunched is not supported on this platform. Falling back to 'Compressed'.", MessageType.Warning);
				} else {
					_data.platformImportDataAr[i].compressionQuality = EditorGUILayout.IntSlider("Compression Quality", _data.platformImportDataAr[i].compressionQuality, 0, 100);
					if(_oldData.platformImportDataAr[i].compressionQuality != _data.platformImportDataAr[i].compressionQuality)
						_hasChanged = true;
				}
			} else if (_data.platformImportDataAr[i].textureImporterFormat == TextureImporterFormat.AutomaticCompressed &&
			           (i == (int)PlatformLabel.iPhone || i == (int)PlatformLabel.Android)) {
				InsertPlatformCompressionQualityBlock(i);

				if(i == (int)PlatformLabel.Android) {
					_data.platformImportDataAr[i].allowsAlphaSplit = GUILayout.Toggle(_data.platformImportDataAr[i].allowsAlphaSplit,
					                                                                           "Compress using ETC1 (split alpha channel)");

					if(_oldData.platformImportDataAr[i].allowsAlphaSplit != _data.platformImportDataAr[i].allowsAlphaSplit)
						_hasChanged = true;
				}
			}

			GUI.enabled = true;
		}

		/// <summary>
		/// Inserts the platform compression quality settings.
		/// </summary>
		/// <param name="index">The platform index</param>
		void InsertPlatformCompressionQualityBlock(int index) {
			int compressionIndex = _data.platformImportDataAr[index].compressionQuality;
			int selectedIndex = 1;
			if(compressionIndex == (int)TextureCompressionQuality.Fast)
				selectedIndex = 0;
			else if (compressionIndex == (int)TextureCompressionQuality.Best)
				selectedIndex = 2;

			selectedIndex = EditorGUILayout.Popup("Compression Quality", selectedIndex, SpriteImporterData.compressionQualityLabels);

			if(selectedIndex == 0)
				_data.platformImportDataAr[index].compressionQuality = (int)TextureCompressionQuality.Fast;
			else if (selectedIndex == 2)
				_data.platformImportDataAr[index].compressionQuality = (int)TextureCompressionQuality.Best;
			else
				_data.platformImportDataAr[index].compressionQuality = (int)TextureCompressionQuality.Normal;

			if(_data.platformImportDataAr[index].compressionQuality != _oldData.platformImportDataAr[index].compressionQuality)
				_hasChanged = true;
		}

		/// <summary>
		/// Clones the asset data.
		/// </summary>
		/// <returns>The cloned postprocessor data copy.</returns>
		/// <param name="asset">The original asset.</param>
		SpriteImporterData CreateImporterDataCopy(SpriteImporterData asset) {
			SpriteImporterData dataCopy = Instantiate(asset);

			dataCopy.platformImportDataAr = new PlatformImportData[asset.platformImportDataAr.Length];
			for(int i = 0; i < asset.platformImportDataAr.Length; i++) {
				dataCopy.platformImportDataAr[i] = new PlatformImportData();
				dataCopy.platformImportDataAr[i].overrideForTargetPlatform = asset.platformImportDataAr[i].overrideForTargetPlatform;
				dataCopy.platformImportDataAr[i].maxSize = asset.platformImportDataAr[i].maxSize;
				dataCopy.platformImportDataAr[i].textureImporterFormat = asset.platformImportDataAr[i].textureImporterFormat;
				dataCopy.platformImportDataAr[i].compressionQuality = asset.platformImportDataAr[i].compressionQuality;
				dataCopy.platformImportDataAr[i].allowsAlphaSplit = asset.platformImportDataAr[i].allowsAlphaSplit;
			}

			return dataCopy;
		}

		/// <summary>
		/// Reimport this all the assets in the folder.
		/// </summary>
		void Reimport() {
			_oldData = CreateImporterDataCopy(_data);
			_hasChanged = false;
			_data.ReimportFolder();
		}

		/// <summary>
		/// Resets all asset attributes to their original values.
		/// </summary>
		void Revert() {
			GUI.FocusControl(null);
			// Sets all attributes to old data
			_data.spriteMode = _oldData.spriteMode;
			_data.packingTag = _oldData.packingTag;
			_data.pixelsPerUnit = _oldData.pixelsPerUnit;
			_data.spriteAlignment = _oldData.spriteAlignment;
			_data.spritePivot = _oldData.spritePivot;
			_data.pivotMapEnabled = _oldData.pivotMapEnabled;
			_data.pivotMapColor = _oldData.pivotMapColor;
			_data.generateMipMaps = _oldData.generateMipMaps;
			_data.filterMode = _oldData.filterMode;
			_data.maxSize = _oldData.maxSize;
			_data.textureImporterFormat = _oldData.textureImporterFormat;
			_data.compressionQuality = _oldData.compressionQuality;

			_data.overrideSpriteEditorSettings = _oldData.overrideSpriteEditorSettings;
			_data.sliceType = _oldData.sliceType;
			_data.sliceWidth = _oldData.sliceWidth;
			_data.sliceHeight = _oldData.sliceHeight;
			_data.sliceBorder = _oldData.sliceBorder;
			_data.sliceCols = _oldData.sliceCols;
			_data.sliceRows = _oldData.sliceRows;
			_data.offset = _oldData.offset;
			_data.padding = _oldData.padding;
			_data.includeBlankSlices = _oldData.includeBlankSlices;

			for(int i = 0; i < _data.platformImportDataAr.Length; i++) {
				_data.platformImportDataAr[i] = new PlatformImportData();
				_data.platformImportDataAr[i].overrideForTargetPlatform = _oldData.platformImportDataAr[i].overrideForTargetPlatform;
				_data.platformImportDataAr[i].maxSize = _oldData.platformImportDataAr[i].maxSize;
				_data.platformImportDataAr[i].textureImporterFormat = _oldData.platformImportDataAr[i].textureImporterFormat;
				_data.platformImportDataAr[i].compressionQuality = _oldData.platformImportDataAr[i].compressionQuality;
				_data.platformImportDataAr[i].allowsAlphaSplit = _oldData.platformImportDataAr[i].allowsAlphaSplit;
			}

			_hasChanged = false;
		}

		#region Styles
		static GUIStyle _backgroundGUIStyle;

		public static GUIStyle BackgroundGUIStyle {
			get{
				if (_backgroundGUIStyle == null)
				{
					_backgroundGUIStyle = new GUIStyle(EditorStyles.textField);
					_backgroundGUIStyle.padding = new RectOffset(5, 5, 5, 5);
				}
				return _backgroundGUIStyle;
			}
		}

		static GUIStyle _toolbarGUIStyle;

		public static GUIStyle ToolbarGUIStyle {
			get{
				if (_toolbarGUIStyle == null)
				{
					_toolbarGUIStyle = new GUIStyle(EditorStyles.toolbarButton);
				}
				return _toolbarGUIStyle;
			}
		}

		static GUIStyle _titleGUIStyle;

		public static GUIStyle TitleGUIStyle {
			get{
				if (_titleGUIStyle == null)
				{
					_titleGUIStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
					_titleGUIStyle.fontSize = 16;
					_titleGUIStyle.fixedHeight = 24f;
					_titleGUIStyle.alignment = TextAnchor.UpperRight;
				}
				if(!EditorGUIUtility.isProSkin)
					_titleGUIStyle.normal.textColor = Color.black;

				return _titleGUIStyle;
			}
		}
		#endregion
	}
}
