﻿using BattleTech;
using BattleTech.Data;
using CustomAmmoCategoriesLog;
using Harmony;
using SVGImporter;
using System;
using System.Collections.Generic;

namespace CustAmmoCategories {
  public class SVGImageLoadDelegate {
    public string id;
    public DataManager dataManager;
    public void onLoad() {
      CustomSvgCache.IconLoaded(id,dataManager);
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("PrewarmComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LoadRequest) })]
  public static class DataManager_PrewarmComplete {
    public static void Postfix(DataManager __instance, LoadRequest batch) {
      Log.M.TWL(0, "DataManager.PrewarmComplete");
      CustomSvgCache.flushRegistredSVGs(__instance);
    }
  }

  public static class CustomSvgCache {
    private static Dictionary<string, SVGAsset> cache = new Dictionary<string, SVGAsset>();
    private static Dictionary<string, HashSet<SVGImage>> defferedRequests = new Dictionary<string, HashSet<SVGImage>>();
    private static HashSet<string> registredSVGIcons = new HashSet<string>();
    public static void RegisterSVG(string id) {
      registredSVGIcons.Add(id);
    }
    public static void flushRegistredSVGs(DataManager dataManager) {
      foreach(string id in registredSVGIcons) {
        if (CustomSvgCache.cache.ContainsKey(id)) { continue; }
        SVGAsset icon = dataManager.GetObjectOfType<SVGAsset>(id,BattleTechResourceType.SVGAsset);
        if (icon != null) {
          Log.M.WL(1, "cache icon:"+id);
          CustomSvgCache.cache.Add(id,icon);
        };
      }
    }
    public static void IconLoaded(string id, DataManager dataManager) {
      SVGAsset icon = get(id, dataManager);
      if (icon == null) { return; }
      if(defferedRequests.TryGetValue(id, out HashSet<SVGImage> images)) {
        foreach(SVGImage image in images) {
          image.vectorGraphics = icon;
        }
        defferedRequests.Remove(id);
      }
    }
    public static void setIcon(SVGImage img, string id, DataManager dataManager) {
      if (img == null) { return; }
      SVGAsset icon = get(id,dataManager);
      if (icon != null) { img.vectorGraphics = icon; return; }
      if(defferedRequests.TryGetValue(id, out HashSet<SVGImage> images) == false) {
        images = new HashSet<SVGImage>();
        defferedRequests.Add(id, images);
        return;
      }
      images.Add(img);
      SVGImageLoadDelegate dl = new SVGImageLoadDelegate();
      dl.id = id;
      DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(dataManager);
      dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, id);
      dependencyLoad.RegisterLoadCompleteCallback(new Action(dl.onLoad));
      dataManager.InjectDependencyLoader(dependencyLoad, 1000U);
    }
    public static SVGAsset get(string id, DataManager dataManager) {
      Log.M.TWL(0, "CustomSvgCache.get " + id);
      if(CustomSvgCache.cache.TryGetValue(id,out SVGAsset result)) {
        Log.M.WL(1, "found in cache");
        return result;
      }
      Log.M.WL(1, "cache content:");
      foreach(var icon in cache) {
        Log.M.WL(2, icon.Key + ":" + (icon.Value == null ? "null" : "not null"));
      }
      result = dataManager.GetObjectOfType<SVGAsset>(id,BattleTechResourceType.SVGAsset);
      if(result != null) {
        Log.M.WL(1, "found in data manager");
        cache.Add(id, result);
      }
      return result;
    }
  }
}