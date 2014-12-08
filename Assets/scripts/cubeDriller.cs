using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;




/*********************************************************** cube data struct */
struct cubeData {
   public bool[,,] chunkStatus;
   public int[,]   countXZ;
   public int[,]   countZY;
   public int[,]   countXY;
   public int      columnCount;
   public Vector3  startPos;
};




/**************************************************************** cubeDriller */
public class cubeDriller : MonoBehaviour {




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   ******************************************************************* config */
   bool DEBUG = false;

   public Camera cam;

   public GameObject prefabChunk;
   public GameObject prefabChunkLabel;

   public TextAsset rulesText;

   public int difficultyMin =  2; // minimum difficulty
   public int difficultyMax = 30; // maximum difficulty
   public int difficulty;         // current difficulty

   public Texture2D menuBackground;

   int difficultySliderValue = -1; // value returned by the GUI difficulty slider

   bool debugToggleValue; // value returned by the GUI debug toggle

   NumberFormatInfo scoreFormat; // score formatting

   System.UInt64 playerScore           = 0; // player score in points
   int           playerConsecutiveWins = 0; // player consecutive wins

   cubeData currentCubeData;

   bool showRules = false; // show/hide rules

   /* game state
      0 - reset game
      1 - playing
      2 - game over
      3 - level transition
      4 - level beaten
      5 - wait, for non frame update
   */
   int gameState = 0;




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   **************************************************************** functions */




   /*************************************************************** init game */
   void gameInit() {

      // no need to hog resources above 60fps
      Application.targetFrameRate = 60;

      // set score formatting
      scoreFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
      scoreFormat.NumberGroupSeparator = " ";

      // set difficulty vars
      difficultySliderValue = difficulty = difficultyMin;

      // set debug toggle value
      debugToggleValue = DEBUG;

   } // end gameInit()




   /************************************************** create menu background */
   void createMenuBackground () {
      menuBackground = new Texture2D(1, 1, TextureFormat.RGB24, false);
      menuBackground.SetPixel(0, 0, Color.black);
      menuBackground.Resize(256,256);
      menuBackground.Apply();
   }




   /********************************************************** user interface */
   void drawGUI() {

      // box styles
      GUIStyle boxStyleMC, boxStyleMR, boxStyleML, boxStyleRules;

      boxStyleMC    = new GUIStyle(GUI.skin.GetStyle("box"));
      boxStyleMR    = new GUIStyle(GUI.skin.GetStyle("box"));
      boxStyleML    = new GUIStyle(GUI.skin.GetStyle("box"));
      boxStyleRules = new GUIStyle(GUI.skin.GetStyle("box"));

      boxStyleMC.alignment = TextAnchor.MiddleCenter;
      boxStyleMR.alignment = TextAnchor.MiddleRight;
      boxStyleML.alignment = TextAnchor.MiddleLeft;

      boxStyleRules.alignment         = TextAnchor.MiddleLeft;
      boxStyleRules.normal.background = menuBackground;


      // title box
      GUI.Box(
         new Rect(5, 5, 140, 60),
         "Cube Driller"
         +"\ndifficulty: "+difficulty
         +"\nremaining columns: "+currentCubeData.columnCount,
         boxStyleML
      );


      // score box
      GUI.Box(
         new Rect(Screen.width-205, 5, 200, 70),
         "score\n"+playerScore.ToString("N0", scoreFormat)
         +"\nconsecutive wins\n"+playerConsecutiveWins,
         boxStyleMR
      );



      // debug toggle
      GUI.Box(new Rect(5, Screen.height-90, 100, 20),"");
      debugToggleValue = GUI.Toggle(
         new Rect(5, Screen.height-90, 100, 20),
         debugToggleValue,
         " enable debug"
      );



      // difficulty slider value display
      GUI.Box(
         new Rect(5, Screen.height-60, 100, 20),
         "difficulty: "+difficultySliderValue,
         boxStyleMC
      );


      // difficulty slider
      difficultySliderValue = (int)GUI.HorizontalSlider(
         new Rect(5, Screen.height-70, 100, 10),
         difficultySliderValue,
         difficultyMin,
         difficultyMax
      );


      // reset game
 		if(GUI.Button(new Rect(10, Screen.height-35, 90, 25), "new game")) {
         gameState = 0;
      }


      // show rules button
 		if(GUI.Button(new Rect(Screen.width-100, Screen.height-30, 95, 25), showRules?"hide rules":"show rules")) {
         showRules = !showRules;
      }


      // show rules
      if(showRules) {
         GUI.Box(
            new Rect(Screen.width/2-225, Screen.height/2-225, 450, 450),
            rulesText.text,
            boxStyleRules
         );
      }


      // version box
      GUI.Box(
         new Rect(Screen.width/2-70, Screen.height-25, 140, 20),
         "alpha-20140315-0452",
         boxStyleMC
      );


      // game over
      if(gameState == 2) {
         GUI.Box(
            new Rect(Screen.width/2-100, Screen.height/2-50, 200, 100),
            "GAME OVER\nThat column was seeminlgy\nless hollow than your head",
            boxStyleMC
         );

         // restart
    		if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2+60, 100, 20), "RESTART")) {
            gameState = 0;
         }

      }


      // level transition, show message for cube generation
      if(gameState == 3) {
         GUI.Box(
            new Rect(Screen.width/2-100, Screen.height/2-50, 200, 100),
            "generating new cube",
            boxStyleMC
         );
      }

   } // end drawGUI()




   /************************************************************** reset game */
   void resetGame() {

      // reset score
      playerConsecutiveWins = 0;
      playerScore           = 0;

      // set difficulty
      difficulty = difficultySliderValue;

      // set debug
      DEBUG = debugToggleValue;


      // change game state level transition
      gameState = 3;

   } // end resetGame()




   /************************************************************ level beaten */
   IEnumerator levelBeaten() {

      gameState = 5; // go in wait state

      float currentScale = 1f;
      float scaleFactor  = 1f;

      // get chunks
      GameObject[] chunks = GameObject.FindGameObjectsWithTag("chunk");

      // get labels
      GameObject[] labels = GameObject.FindGameObjectsWithTag("label");

      while(true) {

         scaleFactor -= 0.8f * Time.deltaTime;
         currentScale *= scaleFactor;

         // animation done, go in transition mode
         if(currentScale < 0.001f) {
            gameState = 3;
            yield break;
         }

         // shrink chunks
         foreach(GameObject chunk in chunks) {
            if(!chunk) {
               continue;
            }
            chunk.transform.localScale = new Vector3(
               chunk.transform.localScale.x * scaleFactor,
               chunk.transform.localScale.y * scaleFactor,
               chunk.transform.localScale.z * scaleFactor
            );
         }


         // shrink labels
         foreach(GameObject label in labels) {
            label.transform.localScale = new Vector3(
               label.transform.localScale.x * scaleFactor,
               label.transform.localScale.y * scaleFactor,
               label.transform.localScale.z * scaleFactor
            );
         }


         yield return null;
      }

   } // end levelTransition()




   /******************************************************** level transition */
   void levelTransition() {

      // create the cube
      createCube();

      // change game state to playing
      gameState = 1;

   } // end levelTransition()




   /******************************************************** create cube data */
   void createCubeData() {

      float startPos = -difficulty / 2f + 0.5f;

      // init new cube data
      currentCubeData = new cubeData();

      currentCubeData.chunkStatus = new bool[difficulty, difficulty, difficulty];

      currentCubeData.countXZ = new int[difficulty, difficulty];
      currentCubeData.countZY = new int[difficulty, difficulty];
      currentCubeData.countXY = new int[difficulty, difficulty];

      currentCubeData.startPos = new Vector3(
         startPos,
         startPos,
         startPos
      );


      // randomly choose a column that must be hollow
      int xRequired = Random.Range(0, difficulty);
      int zRequired = Random.Range(0, difficulty);


      // generate cube data by layers
      for(int yPos=0; yPos<difficulty; yPos++) {


         // create layer
         int rowMax = difficulty;
         for(int xPos=0; xPos<difficulty; xPos++) {
            for(int zPos=0; zPos<difficulty; zPos++) {
               currentCubeData.chunkStatus[xPos, yPos, zPos] = rowMax > zPos ? true : false;
            }
            rowMax--;
         }


         // shuffle layer X rows with random swap
         for(int xPos=0; xPos<difficulty; xPos++) {

            // randomly select the target row
            int xTgt = Random.Range(0, difficulty);

            // swap row with target row
            for(int zPos=0; zPos<difficulty; zPos++) {
               var tgtData = currentCubeData.chunkStatus[xTgt, yPos, zPos];
               currentCubeData.chunkStatus[xTgt, yPos, zPos] = currentCubeData.chunkStatus[xPos, yPos, zPos];
               currentCubeData.chunkStatus[xPos, yPos, zPos] = tgtData;
            }
         }


         // shuffle layer Z rows with random swap
         for(int zPos=0; zPos<difficulty; zPos++) {

            // randomly select the target row
            int zTgt = Random.Range(0, difficulty);

            // swap row with target row
            for(int xPos=0; xPos<difficulty; xPos++) {
               var tgtData = currentCubeData.chunkStatus[xPos, yPos, zTgt];
               currentCubeData.chunkStatus[xPos, yPos, zTgt] = currentCubeData.chunkStatus[xPos, yPos, zPos];
               currentCubeData.chunkStatus[xPos, yPos, zPos] = tgtData;
            }
         }


         // ensure the required chunk is hollow
         if(currentCubeData.chunkStatus[xRequired, yPos, zRequired] == false) {

            // choose randomly if we swap X rows rather than Z rows
            bool xSwap = Random.value > 0.5f ? true : false;

            // default chunk position in layer
            int xPos = xRequired;
            int zPos = zRequired;

            int tgt = -1;


            // find a row with a hollow chunk at the correct position
            for(int pos=0; pos<difficulty; pos++) {

               if(xSwap) {
                  xPos = pos;
               }
               else {
                  zPos = pos;
               }

               // found it, swap
               if(currentCubeData.chunkStatus[xPos, yPos, zPos] == true) {
                  tgt = pos;
                  break;
               }

            }


            // swap the rows
            for(int pos=0; pos<difficulty; pos++) {

               int xSource = xSwap ? xRequired : pos;
               int zSource = xSwap ?       pos : zRequired;

               int xTarget = xSwap ? tgt : pos;
               int zTarget = xSwap ? pos : tgt;

               var tgtData = currentCubeData.chunkStatus[xTarget, yPos, zTarget];
               currentCubeData.chunkStatus[xTarget, yPos, zTarget] = currentCubeData.chunkStatus[xSource, yPos, zSource];
               currentCubeData.chunkStatus[xSource, yPos, zSource] = tgtData;
            }

         }

      }


      // count hollow chunks in all axis
      for(int xPos=0; xPos<difficulty; xPos++) {
         for(int yPos=0; yPos<difficulty; yPos++) {
            for(int zPos=0; zPos<difficulty; zPos++) {
               if(currentCubeData.chunkStatus[xPos, yPos, zPos] == true) {
                  currentCubeData.countXZ[xPos,zPos]++;
                  currentCubeData.countXY[xPos,yPos]++;
                  currentCubeData.countZY[zPos,yPos]++;
               }
            }
         }
      }


      // count hollow columns
      for(int xPos=0; xPos<difficulty; xPos++) {
         for(int zPos=0; zPos<difficulty; zPos++) {
            if(currentCubeData.countXZ[xPos,zPos] == difficulty) {
               currentCubeData.columnCount++;
            }
         }
      }
   }




   /******************************************************* create cube chunk */
   void createCubeChunk(int xPos, int yPos, int zPos) {

      // chunk position
      Vector3 chunkPos = currentCubeData.startPos + new Vector3(
         xPos,
         yPos,
         zPos
      );

      // create object
      GameObject chunk = Instantiate(prefabChunk, chunkPos, Quaternion.identity) as GameObject;

      // set chunk position info
      chunk.GetComponent<chunkHandler>().pos = new int[3]{xPos,yPos,zPos};

      //debug
      if(DEBUG && currentCubeData.chunkStatus[xPos,yPos,zPos] == true) {
         chunk.renderer.material.color = currentCubeData.countXZ[xPos,zPos] == difficulty
            ? new Color(0,0,1)  // hollow column
            : new Color(1,0,0); // hollow chunk
      }
   }



   /************************************************************* create cube */
   void createCube() {

      // reset camera
      cameraReset();


      // generate cube data
      createCubeData();


      // destroy existing cube
      destroyCube();


      // generate visible chunks
      // top layer
      {
         int yPos = difficulty-1;
         for(int xPos=0; xPos<difficulty; xPos++) {
            for(int zPos=0; zPos<difficulty; zPos++) {
               createCubeChunk(xPos, yPos, zPos);
            }
         }
      }


      // left layer
      {
         int xPos = 0;
         for(int yPos=0; yPos<difficulty-1; yPos++) {
            for(int zPos=0; zPos<difficulty; zPos++) {
               createCubeChunk(xPos, yPos, zPos);
            }
         }
      }


      // right layer
      {
         int zPos = 0;
         for(int xPos=1; xPos<difficulty; xPos++) {
            for(int yPos=0; yPos<difficulty-1; yPos++) {
               createCubeChunk(xPos, yPos, zPos);
            }
         }
      }



      // put labels on left side (z,y)
      for(int zPos=0; zPos<difficulty; zPos++) {
         for(int yPos=0; yPos<difficulty; yPos++) {

            Vector3 chunkLabelPos = currentCubeData.startPos + new Vector3(
               -0.5f,
               yPos,
               zPos
            );

            // create label
            GameObject label = Instantiate(prefabChunkLabel, chunkLabelPos, Quaternion.identity) as GameObject;
            label.transform.Rotate(0,90,0);
            label.GetComponent<TextMesh>().text = currentCubeData.countZY[zPos,yPos].ToString();
         }
      }


      // put labels on right side (x,y)
      for(int xPos=0; xPos<difficulty; xPos++) {
         for(int yPos=0; yPos<difficulty; yPos++) {

            Vector3 chunkLabelPos = currentCubeData.startPos + new Vector3(
               xPos,
               yPos,
               -0.5f
            );

            // create label
            GameObject label = Instantiate(prefabChunkLabel, chunkLabelPos, Quaternion.identity) as GameObject;
            label.GetComponent<TextMesh>().text = currentCubeData.countXY[xPos,yPos].ToString();
         }
      }


      // DEBUG: put labels on top side (x,z)
      if(DEBUG) {
         for(int xPos=0; xPos<difficulty; xPos++) {
            for(int zPos=0; zPos<difficulty; zPos++) {

               Vector3 chunkLabelPos = currentCubeData.startPos + new Vector3(
                  xPos,
                  difficulty - 0.5f,
                  zPos
               );

               // create label
               GameObject label = Instantiate(prefabChunkLabel, chunkLabelPos, Quaternion.identity) as GameObject;
               label.GetComponent<TextMesh>().text = currentCubeData.countXZ[xPos,zPos].ToString();
            }
         }
      }
   }




   /************************************************************ destroy cube */
   void destroyCube() {

      // destroy existing cube chunks
      GameObject[] chunks = GameObject.FindGameObjectsWithTag("chunk");
      foreach(GameObject chunk in chunks) {
         Destroy(chunk);
      }

      // destroy existing cube labels
      GameObject[] labels = GameObject.FindGameObjectsWithTag("label");
      foreach(GameObject label in labels) {
         Destroy(label);
      }

   } // end destroyCube()




   /*************************************************** reset camera position */
   void cameraReset() {
      cam.transform.position = new Vector3(0,0,0);
      cam.transform.Translate(0, 0, -50);
      cam.orthographicSize = (float)difficulty;
   } // end cameraReset()




   /********* check if mouse hovers a chunk and get chunkHandler by reference */
   chunkHandler hoverChunk() {

      Ray ray = cam.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;

      return Physics.Raycast(ray, out hit, 100) && hit.collider && hit.collider.tag == "chunk"
         ? hit.transform.gameObject.GetComponent<chunkHandler>()
         : (chunkHandler)null;//;(new GameObject()).AddComponent("chunkHandler").GetComponent<chunkHandler>();

   } // end hoverChunk()




   /******************************************************* player does stuff */
   void playerAction() {

      chunkHandler chunk = null;

      Ray ray = cam.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;

      // get hovered chunk
      if(Physics.Raycast(ray, out hit, 100) && hit.collider && hit.collider.tag == "chunk") {
         chunk = hit.transform.gameObject.GetComponent<chunkHandler>();
      }

      // stop if no chunk or chunk is not in top layer
      if(chunk == null || chunk.pos[1] != difficulty-1) {
         return;
      }

      // highlight chunk
      chunk.highlightChunk();




      // user has clicked a chunk
      if(Input.GetMouseButtonDown(0)) {

         // success: column is hollow
         if(currentCubeData.countXZ[chunk.pos[0],chunk.pos[2]] == difficulty) {

            // set column as solved by changing the count (greater than difficulty, might be checked later again)
            currentCubeData.countXZ[chunk.pos[0],chunk.pos[2]]++;

            // decrease column count
            currentCubeData.columnCount--;

            // set chunk status to destroy
            chunk.destroyChunk();

            // cube is solved
            if(currentCubeData.columnCount == 0) {

               // level transition
               gameState = 4;

               // increase player wins
               playerConsecutiveWins++;

               // increase player score
               playerScore += (System.UInt64)Mathf.Pow(3, difficulty);
            }
         }
         // failure: column is not hollow
         else if(currentCubeData.countXZ[chunk.pos[0],chunk.pos[2]] < difficulty) {

            // game over
            gameState = 2;

         }
      }

   } // end hoverChunk()




   /****************************************************************************
   *****************************************************************************
   *****************************************************************************
   ***************************************************************** internal */
   void Start() {

      gameInit();

   } // end Start()




   void Update() {

      // reset game
      if(gameState == 0) {
         resetGame();
      }
      // playing
      else if(gameState == 1) {
         playerAction();
      }
      // game over
      else if(gameState == 2) {
         //do nothing
      }
      // level transition
      else if(gameState == 3) {
         levelTransition();
      }
      // level beaten
      else if(gameState == 4) {
         StartCoroutine("levelBeaten");
      }
      // nothing, we use this when we update outside frame rendering
      else if(gameState == 5) {
         //do nothing
      }

   } // end Update()




   void OnGUI() {

      drawGUI();

   } // end OnGUI()
}