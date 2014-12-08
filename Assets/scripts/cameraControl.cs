using UnityEngine;
using System.Collections;

public class cameraControl : MonoBehaviour {




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   ******************************************************************* config */
   cubeDriller gameLogic;




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   ***************************************************************** internal */




   void Start() {
      gameLogic = GameObject.Find("gameLogic").GetComponent<cubeDriller>();
   }




   void Update() {

      float speedPan  = 0.05f * (float)gameLogic.difficulty;
      float speedZoom = 10f;

      // get movement (mouse)
      float inputMoveH = Input.GetAxis("Mouse X");
      float inputMoveV = Input.GetAxis("Mouse Y");

      // get panning button (mouse right button)
      bool inputPan = Input.GetKey("space") || Input.GetMouseButton(1);

      // get zoom (mouse wheel)
      float inputZoom = Input.GetAxis("Mouse ScrollWheel");


      // zoom in/out
      if(inputZoom != 0) {
         camera.orthographicSize += inputZoom * speedZoom * -1;
         camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, gameLogic.difficultyMin, gameLogic.difficultyMax);
      }


      // pan "horizontally"
      if(inputPan && inputMoveH != 0) {
         transform.Translate(inputMoveH * speedPan * -1, 0, 0);
      }


      // pan "vertically"
      if(inputPan && inputMoveV != 0) {
         transform.Translate(0, inputMoveV * speedPan * -1, 0);
      }

   }

}