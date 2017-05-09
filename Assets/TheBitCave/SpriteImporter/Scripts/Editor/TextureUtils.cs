using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility class for checking and handling textures.
/// </summary>
namespace TheBitCave.SpriteImporter {

	public class TextureUtils {

		internal static bool IsSpriteTexture(Object obj) {
			string assetPath = AssetDatabase.GetAssetPath(obj);
			Texture2D asset = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
			if(asset != null) {
				TextureImporter textureImporter = (TextureImporter) TextureImporter.GetAtPath(assetPath);
				return textureImporter.textureType == TextureImporterType.Sprite;
			}
			return false;
		}
		
		internal static bool IsTexture(Object obj) {
			string assetPath = AssetDatabase.GetAssetPath(obj);
			Texture2D asset = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
			if(asset != null) {
				return true;
			}
			return false;
		}

		internal static bool IncludesAtLeastATexture(Object[] objects) {
			foreach(Object obj in objects) {
				if (IsTexture(obj))
					return true;
			}
			return false;
		}

		internal static bool IncludesAllTextures(Object[] objects) {
			foreach(Object obj in objects) {
				if (!IsTexture(obj)) {
					return false;
				}
			}
			return true;
		}

		internal static void ApplyTemplateToTextures(Object[] objects, string templateName) {
			Texture2D texture;
			foreach(Object obj in objects) {
				string assetPath = AssetDatabase.GetAssetPath(obj);
				texture = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
				if(texture != null) {
					ApplyTemplateToTexture(texture, templateName);
				}
			}
		}

		internal static void ApplyTemplateToTexture(Texture2D texture, string templateName) {
			SpriteImporterData data = TemplateUtils.GetTemplateData(templateName);
			string assetPath = AssetDatabase.GetAssetPath(texture);
			TextureImporter textureImporter = (TextureImporter) TextureImporter.GetAtPath(assetPath);

			if(data == null)
				return;

			ImportTexture(data, textureImporter);
			SetSlicingSettings(data, texture, textureImporter);

			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Imports the texture using the settings from the scriptable object.
		/// </summary>
		/// <param name="importerData">The scriptable object containing all import data.</param>
		internal static void ImportTexture(SpriteImporterData importerData, TextureImporter textureImporter) {
			TextureImporterSettings settings = new TextureImporterSettings();
			textureImporter.ReadTextureSettings(settings);

			settings.ApplyTextureType(TextureImporterType.Sprite, true);
			settings.spriteMode = importerData.spriteMode;
			settings.spritePixelsPerUnit = importerData.pixelsPerUnit;
			settings.spriteAlignment = importerData.spriteAlignment;
			settings.spritePivot = importerData.spritePivot;
			if(importerData.pivotMapEnabled && settings.spriteMode == SpriteImporterData.spriteModes[0]) {
				string pivotMapPath = GetPivotMapPath(textureImporter.assetPath);
				Texture2D mapTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(pivotMapPath, typeof(Texture2D));
				if(mapTexture != null) {
					Vector2? pivotPosition = GetPivotPosition(mapTexture, importerData.pivotMapColor);
					if(pivotPosition != null) {
						settings.spriteAlignment = (int)SpriteAlignment.Custom;
						settings.spritePivot = new Vector2(pivotPosition.Value.x / mapTexture.width, pivotPosition.Value.y / mapTexture.height);
					}
				}
			}
			settings.mipmapEnabled = importerData.generateMipMaps;
			settings.filterMode = importerData.filterMode;
			settings.maxTextureSize = importerData.maxSize;
			settings.textureFormat = importerData.textureImporterFormat;
			settings.allowsAlphaSplit = importerData.platformImportDataAr[(int)PlatformLabel.Android].allowsAlphaSplit;

			textureImporter.SetTextureSettings(settings);
			textureImporter.spritePackingTag = importerData.packingTag;
			textureImporter.textureType = TextureImporterType.Sprite;
			textureImporter.compressionQuality = importerData.compressionQuality;

			for (int i = 0; i < importerData.platformImportDataAr.Length; i++) {
				if(importerData.platformImportDataAr[i].overrideForTargetPlatform) {
					textureImporter.SetPlatformTextureSettings(SpriteImporterData.platformLabels[i + 1],
					                                           importerData.platformImportDataAr[i].maxSize,
					                                           importerData.platformImportDataAr[i].textureImporterFormat,
					                                           importerData.platformImportDataAr[i].compressionQuality,
					                                           importerData.platformImportDataAr[i].allowsAlphaSplit);
				} else {
					textureImporter.ClearPlatformTextureSettings(SpriteImporterData.platformLabels[i + 1]);
				}
			}
		}

		/// <summary>
		/// Gets the pivot position from the map texture.
		/// </summary>
		/// <returns>The pivot position.</returns>
		/// <param name="mapTexture">The texture containing the pivot map.</param>
		/// <param name="pivotColor">The pivot map color.</param>
		internal static Vector2? GetPivotPosition(Texture2D mapTexture, Color pivotMapColor) {
			Vector2? pivotPosition = null;
			Color[] pixels = mapTexture.GetPixels();
			int rowCount = 0;
			int colCount = 0;
			foreach(Color p in pixels) {
				if(System.Math.Round(p.r, 4) == System.Math.Round(pivotMapColor.r, 4) &&
				   System.Math.Round(p.g, 4) == System.Math.Round(pivotMapColor.g, 4) &&
				   System.Math.Round(p.b, 4) == System.Math.Round(pivotMapColor.b, 4) &&
				   System.Math.Round(p.a, 4) == System.Math.Round(pivotMapColor.a, 4)) {
					pivotPosition = new Vector2(colCount, rowCount);
					break;
				}
				
				colCount++;
				if(colCount >= (int)mapTexture.width) {
					colCount = 0;
					rowCount++;
				}
			}
			return pivotPosition;
		}

		/// <summary>
		/// If the slice texture has been enabled, set the slicing metadata
		/// </summary>
		/// <param name="importerData">Importer data.</param>
		/// <param name="texture2D">Texture 2D.</param>
		/// <param name="textureImporter">Texture importer.</param>
		internal static void SetSlicingSettings(SpriteImporterData importerData, Texture2D texture2D, TextureImporter textureImporter) {
			if(textureImporter.assetPath.Contains(SpriteImporterData.ThePivotMapSuffix)) {
				return;
			}
			TextureImporterSettings settings = new TextureImporterSettings();
			textureImporter.ReadTextureSettings(settings);
			bool sliceTexture = settings.spriteMode == SpriteImporterData.spriteModes[1] && importerData.overrideSpriteEditorSettings;

			if(importerData != null && importerData.importerEnabled && sliceTexture) {
				textureImporter.isReadable = true;

				List<SpriteMetaData> spritesData = new List<SpriteMetaData>();
				SpriteMetaData metaData;
				int sliceCount = 0;
				int cols;
				int rows;
				
				if(importerData.sliceType == SliceType.GridByCellCount) {
					cols = importerData.sliceCols;
					rows = importerData.sliceRows;
					importerData.sliceWidth = (int)((texture2D.width - importerData.offset.x) / cols - importerData.padding.x);
					importerData.sliceHeight = (int)((texture2D.height - importerData.offset.y) / rows - importerData.padding.y);
				} else {
					cols = (int)texture2D.width / importerData.sliceWidth;
					rows = (int)texture2D.height / importerData.sliceHeight;
				}
				int restY = texture2D.height % importerData.sliceHeight;
				Rect sliceRect;
				Rect textureRect = new Rect(0, 0, texture2D.width, texture2D.height);

				string pivotMapPath = GetPivotMapPath(textureImporter.assetPath);
				Texture2D mapTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(pivotMapPath, typeof(Texture2D));

				for (int j = 0; j < rows; j++)
				{
					for(int i = 0; i < cols;  i++)
					{
						metaData = new SpriteMetaData();
						metaData.alignment = importerData.spriteAlignment;
						metaData.pivot = importerData.spritePivot;
						metaData.border = importerData.sliceBorder;
						metaData.name = GetTextureNameFromPath(textureImporter.assetPath) + "_" + sliceCount;
						sliceRect = new Rect(i * (importerData.sliceWidth + importerData.padding.x) + importerData.offset.x,
						                     (rows - j - 1) * importerData.sliceHeight - (j * importerData.padding.y) + restY - importerData.offset.y,
						                     importerData.sliceWidth,
						                     importerData.sliceHeight);
						metaData.rect = sliceRect;

						if(sliceRect.x >= 0 && sliceRect.x <= textureRect.width &&
						   sliceRect.x + sliceRect.width <= textureRect.width &&
						   sliceRect.y >= 0 && sliceRect.y <= textureRect.height &&
						   sliceRect.y + sliceRect.height <= textureRect.height) {
							Color[] pixels = texture2D.GetPixels((int)sliceRect.x, (int)sliceRect.y, (int)sliceRect.width, (int)sliceRect.height);

							if(importerData.pivotMapEnabled && mapTexture != null) {
								Color[] p = mapTexture.GetPixels((int)sliceRect.x, (int)sliceRect.y, (int)sliceRect.width, (int)sliceRect.height);
								Texture2D singlePivotTexture = new Texture2D((int)sliceRect.width, (int)sliceRect.height);
								singlePivotTexture.SetPixels(p);
								singlePivotTexture.Apply();
								Vector2? pivotPosition = GetPivotPosition(singlePivotTexture, importerData.pivotMapColor);
								if(pivotPosition != null) {
									metaData.alignment = (int)SpriteAlignment.Custom;
									metaData.pivot = new Vector2(pivotPosition.Value.x / singlePivotTexture.width, pivotPosition.Value.y / singlePivotTexture.height);
								}
							}

							bool isTransparent = IsTransparent(pixels);
							if(!isTransparent || importerData.includeBlankSlices) {
								spritesData.Add(metaData);
								sliceCount++;
							}
						}
					}
				}
				textureImporter.spritesheet = spritesData.ToArray();
				textureImporter.isReadable = false;
			}
		}

		/// <summary>
		/// Determines if an array of pixels is all transparent.
		/// </summary>
		/// <returns><c>true</c> if all pixels in the array are transparent; otherwise, <c>false</c>.</returns>
		/// <param name="pixels">The pixels array.</param>
		internal static bool IsTransparent(Color[] pixels) {
			bool isTransparent = true;
			foreach(Color p in pixels) {
				if(p.a != 0) {
					isTransparent = false;
					break;
				}
			}
			return isTransparent;
		}

		/// <summary>
		/// Gets the texture name from path.
		/// </summary>
		/// <returns>The texture name from path.</returns>
		/// <param name="path">Path.</param>
		internal static string GetTextureNameFromPath(string path) {
			string[] ar = path.Split('/');
			ar = ar[ar.Length - 1].Split('.');
			List<string> list = ar.ToList();
			list.RemoveAt(list.Count - 1);
			return string.Join(".", list.ToArray());
		}

		/// <summary>
		/// Gets the pivot Map path.
		/// </summary>
		/// <returns>The pivot Map path</returns>
		/// <param name="path">Path.</param>
		internal static string GetPivotMapPath(string path) {
			int pos = path.LastIndexOf('.');
			return path.Insert(pos, SpriteImporterData.ThePivotMapSuffix);
		}
	}

}