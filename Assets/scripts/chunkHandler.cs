using UnityEngine;
using System.Collections;

public class chunkHandler : MonoBehaviour {




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   ******************************************************************* config */
   public Material matHighlight;
   public Material matClickHighlight;
   public Material matDestroyed;

   public int[] pos; // chunk position x,y,z

   Material matStart;

   bool isHiglighted = false;
   bool isEnabled    = true;




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   **************************************************************** functions */




   public void highlightChunk() {
      isHiglighted = true;
   }




   public void destroyChunk() {
      isEnabled = false;
   }




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   ***************************************************************** internal */




   void Start() {
      matStart = renderer.material; // remember original material
   }




   void Update() {

      // not enabled and wrong material, apply disabled material
      if(!isEnabled && renderer.material != matDestroyed) {
         renderer.material = matDestroyed;
      }
      // enabled, highlighted and wrong material, apply highlight material
      else if(isEnabled && isHiglighted && renderer.material != matClickHighlight) {
         renderer.material = matClickHighlight;
         isHiglighted = false; // disable highlighting for next frame
      }
      // wrong material, apply default material
      else if(renderer.material != matStart) {
         renderer.material = matStart;
      }

   }

}