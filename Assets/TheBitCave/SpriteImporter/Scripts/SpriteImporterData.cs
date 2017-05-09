//----------------------------------------------
//            Sprite Importer
//       Copyright © 2015 Marco Secchi
//           http://thebitcave.com
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Scriptable Object used to store texture import settings inside a folder.
/// </summary>
/// 
namespace TheBitCave.SpriteImporter {

	public class SpriteImporterData : ScriptableObject {

		public const string TheImporterDataName = "tbc_import_settings";
		public const string TheImporterDataAssetName = TheImporterDataName + ".asset";
		public const string ThePivotMapSuffix = "_pivotmap";

		public bool importerEnabled = true;

		/// <summary>
		/// The Sprite packing tag name.
		/// </summary>
		public string packingTag = "";

		/// <summary>
		/// The number of pixels in the sprite that correspond to one unit
		/// in world space.
		/// </summary>
		public float pixelsPerUnit = 100;

		/// <summary>
		/// Enabled/Disables mipmaps.
		/// </summary>
		public bool generateMipMaps = false;

		public void Init() {
			for(int i = 0; i < platformImportDataAr.Length; i++) {
				platformImportDataAr[i] = platformImportDataAr[i] ?? new PlatformImportData();
			}
		}
		
		#region Sprite Mode

		/// <summary>
		/// A list of available labels for sprite modes.
		/// </summary>
		public static readonly string[] spriteModeLabels = {"Single", "Multiple"};
		
		/// <summary>
		/// The sprite modes used by unity: '1' for 'Single' and '2' for 'Multiple'
		/// ('0' is reserved for 'none' and not used by sprites)
		/// </summary>
		public static readonly int[] spriteModes = {1, 2};
		
		public int spriteMode = 1; 

		#endregion

		#region Filter Mode
		
		/// <summary>
		/// A list of available labels for filter modes.
		/// </summary>
		public static readonly string[] filterModeLabels = {"Bilinear", "Point (no filter)", "Trilinear"};		

		public FilterMode filterMode = FilterMode.Point;

		FilterMode[] _filterModes = new[] {FilterMode.Bilinear, FilterMode.Point, FilterMode.Trilinear};
		
		public FilterMode[] FilterModes {
			get {
				return _filterModes;
			}
		}

		#endregion

		#region Pivot Point
		
		/// <summary>
		/// A list of available labels for pivot points.
		/// </summary>
		public static readonly string[] pivotPointLabels = {"Center", "Top Left", "Top", "Top Right", "Left", "Right", "Bottom Left", "Bottom", "Bottom Right", "Custom"};

		public Vector2 spritePivot = new Vector2(.5f, .5f);

		public int spriteAlignment = 0;

		public bool pivotMapEnabled = false;

		public Color pivotMapColor = Color.red;

		#endregion

		#region Max Size and Format
		
		/// <summary>
		/// A list of available labels max sizes.
		/// </summary>
		public static readonly string[] maxSizeLabels = {"32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"};

		/// <summary>
		/// A list of available labels for texture importer formats.
		/// </summary>
		public static readonly string[] textureImporterFormatLabels = {"Compressed", "16 bits", "Truecolor", "Crunched"};

		/// <summary>
		/// A list of the available target platforms
		/// </summary>
		public static readonly string[] platformLabels = {"Default", "Web", "Standalone", "iPhone", "Android", "Tizen", "WebGL", "Samsung TV"};

		/// <summary>
		/// Compression quality labels used for iPhone 'Compressed' format.
		/// </summary>
		public static readonly string[] compressionQualityLabels = {"Fast", "Normal", "Best"};

		/// <summary>
		/// The multiple sprite mode slice types.
		/// </summary>
		public static readonly string[] sliceTypeLabels = {"Grid By Cell Size", "Grid By Cell Count"};
		
		public int maxSize = 2048;

		public PlatformImportData[] platformImportDataAr = new PlatformImportData[7];

		/// <summary>
		/// The compression quality for Crunched texture format.
		/// </summary>
		public int compressionQuality = 50;
		
		public TextureImporterFormat textureImporterFormat = TextureImporterFormat.AutomaticTruecolor;

		TextureImporterFormat[] _textureImporterFormats = new[] {TextureImporterFormat.AutomaticCompressed, TextureImporterFormat.Automatic16bit, TextureImporterFormat.AutomaticTruecolor, TextureImporterFormat.AutomaticCrunched};

		public TextureImporterFormat[] TextureImporterFormats {
			get {
				return _textureImporterFormats;
			}
		}

		#endregion

		#region Multiple Sprite Mode settings

		public bool overrideSpriteEditorSettings = false;

		public int sliceWidth = 64;
		public int sliceHeight = 64;
		public int sliceCols = 1;
		public int sliceRows = 1;
		public Vector4 sliceBorder = new Vector4(0, 0, 0, 0);
		public Vector2 offset = new Vector2(0, 0);
		public Vector2 padding = new Vector2(0, 0);

		public bool includeBlankSlices = false;

		public SliceType sliceType = SliceType.GridByCellSize;

		SliceType[] _sliceTypes = new[] {SliceType.GridByCellSize, SliceType.GridByCellCount};
		
		public SliceType[] SliceTypes {
			get {
				return _sliceTypes;
			}
		}

		#endregion

		/// <summary>
		/// Reimports the full folder. Usually used after the post processor data has changed.
		/// </summary>
		public void ReimportFolder() {
			string assetName = "/" + TheImporterDataAssetName;
			string folderPath = AssetDatabase.GetAssetPath(this).Replace(assetName, "");
			AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
		}
	}

	/// <summary>
	/// Compression and max size data container for each supported platform.
	/// </summary>
	[System.Serializable]
	public class PlatformImportData {
		public bool overrideForTargetPlatform = false;
		public TextureImporterFormat textureImporterFormat = TextureImporterFormat.AutomaticTruecolor;
		public int maxSize = 2048;
		public int compressionQuality = 50;
		public bool allowsAlphaSplit = false;
	}

	/// <summary>
	/// Platform specific names to be used with platformLabels array.
	/// </summary>
	public enum PlatformLabel {
		Web = 0,
		Standalone = 1,
		iPhone = 2,
		Android = 3,
		Tizen = 4,
		WebGL = 5,
		SamsungTV = 6
	}

	/// <summary>
	/// The slice type for the multi-sprites slicing.
	/// </summary>
	public enum SliceType {
		GridByCellSize,
		GridByCellCount
	}
	

}
