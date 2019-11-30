﻿using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.UrbanWarfare;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class MultiShotPulseEffect : CopyAbleWeaponEffect {
    public MultiShotPPCEffect parentProjector;
    public bool primePulse;
    public int pulseIdx;
    public Material ppcBeamFake;
    public Material ppcBeamCoil;
    public Material zapArcFlip1;
    public Material PPCspark;
    public Material ppcTrail;
    public Material glowBlue;
    /*public Color ppcTrail_color;
    public Color ppcBeamCoil_color;
    public Color zapArcFlip1_color;
    public Color PPCspark_color;
    public Color glowBlue_color;*/
    public override void StoreOriginalColor() {
      Log.M.TWL(0, "MultiShotPulseEffect.StoreOriginalColor");
      ParticleSystemRenderer[] renderers = this.projectile.GetComponentsInChildren<ParticleSystemRenderer>();
      ppcTrail = null;
      ppcBeamCoil = null;
      zapArcFlip1 = null;
      PPCspark = null;
      glowBlue = null;
      foreach (ParticleSystemRenderer renderer in renderers) {
        Log.LogWrite(" " + renderer.name + ": materials\n");
        foreach (Material material in renderer.materials) {
          Log.LogWrite("  " + material.name + ": " + material.color + " : "+material.ColorBB()+"\n");
          if (material.name.StartsWith("vfxMatPrtl_ppcBeamFake_alpha")) {
            this.ppcBeamFake = material; material.RegisterRestoreColor();
          } else
          if (material.name.StartsWith("vfxMatPrtl_ppcBeamCoil_alpha")) {
            this.ppcBeamCoil = material; material.RegisterRestoreColor();
          } else
          if (material.name.StartsWith("vfxMatPrtl_zapArcFlip1_blue_alpha")) {
            this.zapArcFlip1 = material; material.RegisterRestoreColor();
          } else
          if (material.name.StartsWith("vfxMatPrtl_PPCspark_alpha")) {
            this.PPCspark = material; material.RegisterRestoreColor();
          } else
          if (material.name.StartsWith("vfxMatPrtl_electricTrail_alpha")) {
            this.ppcTrail = material; material.RegisterRestoreColor();
          } else
          if (material.name.StartsWith("vfxMatPrtl_glowBlue_alpha")) {
            this.glowBlue = material; material.RegisterRestoreColor();
          }
        }
      }
    }
    public override void RestoreOriginalColor() {
      /*if (ppcTrail != null) ppcTrail.SetColor("_ColorBB", ppcTrail_color);
      if (ppcBeamCoil != null) ppcBeamCoil.SetColor("_ColorBB", ppcBeamCoil_color);
      if (zapArcFlip1 != null) zapArcFlip1.SetColor("_ColorBB", zapArcFlip1_color);
      if (PPCspark != null) PPCspark.SetColor("_ColorBB", PPCspark_color);
      if (glowBlue != null) glowBlue.SetColor("_ColorBB", glowBlue_color);*/
    }
    public override void SetColor(Color color) {
      //Log.M.TWL(0, "MultiShotPulseEffect.SetColor: "+color);
      color.a = 1f;
      float coeff = color.maxColorComponent;
      Color tempColor = Color.white;
      tempColor.a = 1f;
      tempColor.r = (color.r / coeff) * 5.0f + 1f;
      tempColor.g = (color.g / coeff) * 5.0f + 1f;
      tempColor.b = (color.b / coeff) * 5.0f + 1f;
      if (ppcBeamFake != null) ppcBeamFake.SetColor("_ColorBB", tempColor);
      tempColor.r = (color.r / coeff) * 6.0f + 1f;
      tempColor.g = (color.g / coeff) * 6.0f + 1f;
      tempColor.b = (color.b / coeff) * 6.0f + 1f;
      if (ppcBeamCoil != null) ppcBeamCoil.SetColor("_ColorBB", tempColor);
      if (zapArcFlip1 != null) zapArcFlip1.SetColor("_ColorBB", tempColor);
      tempColor.r = (color.r / coeff) * 5.0f + 2f;
      tempColor.g = (color.g / coeff) * 5.0f + 2f;
      tempColor.b = (color.b / coeff) * 5.0f + 2f;
      if (glowBlue != null) glowBlue.SetColor("_ColorBB", tempColor);
      tempColor.r = (color.r / coeff) * 6.5f + 1.5f;
      tempColor.g = (color.g / coeff) * 6.5f + 1.5f;
      tempColor.b = (color.b / coeff) * 6.5f + 1.5f;
      if (PPCspark != null) PPCspark.SetColor("_ColorBB", tempColor);
      tempColor.r = (color.r / coeff) * 7.0f + 1.3f;
      tempColor.g = (color.g / coeff) * 7.0f + 1.3f;
      tempColor.b = (color.b / coeff) * 7.0f + 1.3f;
      if (ppcTrail != null) ppcTrail.SetColor("_ColorBB", tempColor);
      //Log.M.WL(1, ppcBeamFake.name+":"+ ppcBeamFake.ColorBB());
      //Log.M.WL(1, ppcBeamCoil.name + ":" + ppcBeamCoil.ColorBB());
      //Log.M.WL(1, zapArcFlip1.name + ":" + zapArcFlip1.ColorBB());
      //Log.M.WL(1, glowBlue.name + ":" + glowBlue.ColorBB());
      //Log.M.WL(1, PPCspark.name + ":" + PPCspark.ColorBB());
      //Log.M.WL(1, ppcTrail.name + ":" + ppcTrail.ColorBB());
    }
    protected override int ImpactPrecacheCount {
      get {
        return 1;
      }
    }

    protected override void Awake() {
      base.Awake();
    }

    protected override void Start() {
      base.Start();
    }
    public void Init(PPCEffect original) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("MultiShotPPCEffect.Init\n");
    }

    public void Init(Weapon weapon, MultiShotPPCEffect parentProjector) {
      this.Init(weapon);
      this.parentProjector = parentProjector;
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.projectileSpeed = parentProjector.projectileSpeed * weapon.ProjectileSpeedMultiplier();
      this.subEffect = true;
    }

    public virtual void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0, bool pb = false) {
      Log.LogWrite("MultiShotPPCEffect.Fire " + hitInfo.attackWeaponIndex + " " + hitIndex + " ep:" + hitInfo.hitPositions[hitIndex] + " prime:" + pb + "\n");
      this.primePulse = pb;
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:" + this.endPos + "\n");
      endPos.x += Random.Range(-this.parentProjector.spreadAngle, this.parentProjector.spreadAngle);
      endPos.y += Random.Range(-this.parentProjector.spreadAngle, this.parentProjector.spreadAngle);
      endPos.z += Random.Range(-this.parentProjector.spreadAngle, this.parentProjector.spreadAngle);
      this.endPos = endPos;
      float num = Vector3.Distance(this.startingTransform.position, this.endPos);
      if ((double)this.projectileSpeed > 0.0)
        this.duration = num / this.projectileSpeed;
      else
        this.duration = 1f;
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }

    protected override void PlayPreFire() {
      base.PlayPreFire();
      int num = (int)WwiseManager.PostEvent<AudioEventList_ppc>(AudioEventList_ppc.ppc_shoot, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
    }

    public override void InitProjectile() {
      base.InitProjectile();
      Log.LogWrite("MultiShotPulseEffect.InitProjectile\n");
      Component[] components = this.projectile.GetComponentsInChildren<Component>();
      foreach (Component component in components) {
        Log.LogWrite(" " + component.name + ":" + component.GetType().ToString() + "\n");
      }
    }

    protected override void PlayProjectile() {
      this.PlayMuzzleFlash();
      base.PlayProjectile();
    }

    protected override void PlayImpact() {
      this.PlayImpactAudio();
      base.PlayImpact();
      if (!((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null))
        return;
      this.projectileParticles.Stop(true);
    }

    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
        return;
      if ((double)this.t < 1.0) {
        this.currentPos = Vector3.Lerp(this.startPos, this.endPos, this.t);
        this.projectileTransform.position = this.currentPos;
        this.UpdateColor();
      }
      if ((double)this.t < 1.0)
        return;
      this.PlayImpact();
      this.OnComplete();
    }

    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }

#if BT1_8
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
#else
    protected override void OnImpact(float hitDamage = 0.0f) {
#endif
      Log.LogWrite("MultiShotPulseEffect.OnImpact wi:" + this.hitInfo.attackWeaponIndex + " hi:" + this.hitInfo + " bi:" + this.pulseIdx + " prime:" + this.primePulse + "\n");
      if (this.primePulse) {
        Log.LogWrite(" prime. Damage message fired\n");
        float damage = this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
#if BT1_8
        float apDamage = this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
#endif
        if (this.weapon.DamagePerPallet()&&(this.weapon.DamageNotDivided() == false)) {
          damage /= this.weapon.ProjectilesPerShot;
#if BT1_8
          apDamage /= this.weapon.ProjectilesPerShot;
#endif
        };
#if BT1_8
        base.OnImpact(damage, apDamage);
#else
        base.OnImpact(damage);
#endif
      } else {
        Log.LogWrite(" no prime. No damage message fired\n");
      }
      this.RestoreOriginalColor();
      if (!((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)) { return; };
      this.projectileParticles.Stop(true);
    }

    protected override void OnComplete() {
      base.OnComplete();
    }

    public override void Reset() {
      base.Reset();
    }
  }
}