using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagnumFoundation2.Objects;

namespace MagnumFoundation2.System
{
    public class s_camera : s_singleton<s_camera>
    {
        public GameObject player;
        public static s_camera cam;
        public Vector2 offset;
        float offset_multiplier;
        public float orthGraphicSize = 0;
        public float orthGraphicSizeDefault = 0;

        public Vector2 mapSize;
        public Vector2 tileSize;

        public Vector3 targetPos;
        Vector3 startPos;
        public float speedProg = 0;
        public float speedProgInc = 0.25f;
        
        public bool camWithMouseOffset = false;
        public static Vector2 position;

        float targetZoom;
        private float camerashakeDel = 0;
        private float cameraoffset_X;
        private float cameraoffset_Y;
        Camera gameCam;

        public bool alwaysRunning = true;

        int coroutineID = 0;

        public enum CAMERA_MODE {
            CHARACTER_FOCUS,
            LERPING,
            ZOOM,
            LERP_ZOOM,
            CHARACTER_FOCUS_ZOOM,
            NONE = -1
        }
        public CAMERA_MODE cameraMode;

        void Awake()
        {
            if (cam == null)
                cam = this;
            DontDestroyOnLoad(this);
            gameCam = GetComponent<Camera>();
        }

        void Start()
        {
            if (gameCam != null)
            {
                orthGraphicSize = gameCam.orthographicSize * 2;
                orthGraphicSizeDefault = orthGraphicSize;
            }
        }

        public void SetPlayerFocus(GameObject cha)
        {
            alwaysRunning = true;
            player = cha;
            cameraMode = CAMERA_MODE.CHARACTER_FOCUS;
        }

        public void SetPlayer(GameObject cha)
        {
            player = cha;
        }

        public void SetTargPos(Vector2 pos, float spd)
        {
            startPos =  new Vector3(transform.position.x, transform.position.y, - 5);
            cameraMode = CAMERA_MODE.LERPING;
            targetPos = new Vector3(pos.x, pos.y, -5);
            speedProg = spd;
        }
        public void ZoomCameraMove(float startZoom, float zoom, Vector2 pos)
        {
            targetPos = new Vector3(pos.x, pos.y, -5);
            gameCam.orthographicSize = startZoom;
            cameraMode = CAMERA_MODE.LERP_ZOOM;
            targetZoom = zoom;
            speedProg = 0;
        }

        /*
        public void ZoomCamera(float startZoom, float zoom)
        {
            gameCam.orthographicSize = startZoom;
            cameraMode = CAMERA_MODE.ZOOM;
            targetZoom = zoom;
            speedProg = 0;
        }
        */

        public void SetZoom()
        {
            cameraMode = CAMERA_MODE.NONE;
            gameCam.orthographicSize = orthGraphicSizeDefault / 2;
        }
        
        public void SetZoom(float zoom)
        {
            cameraMode = CAMERA_MODE.NONE;
            gameCam.orthographicSize = zoom;
        }

        public void ZoomCamera(float zoom)
        {
            cameraMode = CAMERA_MODE.ZOOM;
            targetZoom = zoom;
            if (zoom < 0)
                gameCam.orthographicSize = orthGraphicSizeDefault / 2;
        }
        public void InstantZoomCamera(float zoom)
        {
            alwaysRunning = true;
            gameCam.orthographicSize = zoom;
        }

        public void TeleportCamera(Vector2 pos)
        {
            cameraMode = CAMERA_MODE.NONE;
            transform.position = new Vector3(pos.x, pos.y, -5);
        }

        public void ResetSpeedProg()
        {
            speedProg = 0;
        }

        public Vector2 GetCentroid(List<Vector2> positions) {
            float dx = 0;
            float dy = 0;

            foreach (Vector2 pos in positions) {
                dx += pos.x;
                dy += pos.y;
            }

            float _x = dx / positions.Count;
            float _y = dy / positions.Count;

            return new Vector2(_x, _y);
        }

        public IEnumerator MoveCamera(Vector2 targ, float speed)
        {
            cameraMode = CAMERA_MODE.NONE;
            alwaysRunning = false;

            coroutineID++;
            int initId = coroutineID;

            targetPos = targ;
            float spd = 0;
            Vector3 trg = new Vector3(
                    targetPos.x,
                    targetPos.y,
                    transform.position.z);

            while (spd <= 0.99f * speed)
            {
                if (coroutineID != initId)
                    yield break;
                spd += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(transform.position, trg, spd);
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        public IEnumerator ZoomCameraPaused(float targ, float speed)
        {
            alwaysRunning = false;
            cameraMode = CAMERA_MODE.NONE;
            float spd = 0;
            coroutineID++;
            int initId = coroutineID;

            if (targ < 0)
                targ = orthGraphicSizeDefault / 2;

            while (spd <= 0.99f * speed)
            {
                if (coroutineID != initId)
                    yield break;
                gameCam.orthographicSize = Mathf.Lerp(gameCam.orthographicSize, targ, spd);
                spd += Time.deltaTime * speed;
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
        public IEnumerator ZoomCamera(float targ, float speed)
        {
            alwaysRunning = false;
            cameraMode = CAMERA_MODE.NONE;
            float spd = 0;
            coroutineID++; 
            int initId = coroutineID;

            if (targ < 0)
                targ = orthGraphicSizeDefault / 2;

            while (spd <= 0.99f * speed)
            {
                if (coroutineID != initId)
                    yield break;
                gameCam.orthographicSize = Mathf.Lerp(gameCam.orthographicSize, targ, spd);
                spd += Time.deltaTime * speed;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        public IEnumerator ZoomCamera(float targ,Vector2 targetP, float speed)
        {
            alwaysRunning = false;
            cameraMode = CAMERA_MODE.NONE;
            float spd = 0;
            Vector3 trgP = new Vector3(
                    targetP.x,
                    targetP.y,
                    transform.position.z);

            coroutineID++;
            int initId = coroutineID;

            if (targ < 0)
                targ = orthGraphicSizeDefault / 2;

            while (spd <= 0.99f * speed)
            {
                if (coroutineID != initId)
                    yield break;
                gameCam.orthographicSize = Mathf.Lerp(gameCam.orthographicSize, targ, spd);
                transform.position = Vector3.Lerp(transform.position, trgP, spd);
                spd += Time.deltaTime * speed;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            position = transform.position;
            if (alwaysRunning)
            {
                switch (cameraMode)
                {
                    case CAMERA_MODE.LERP_ZOOM:
                        gameCam.orthographicSize = Mathf.Lerp(gameCam.orthographicSize, targetZoom, speedProg);
                        transform.position = Vector3.Lerp(transform.position, targetPos, speedProg) + new Vector3(0, 0, -5);
                        speedProg += speedProgInc * Time.deltaTime;
                        break;

                    case CAMERA_MODE.ZOOM:
                        break;

                    case CAMERA_MODE.LERPING:
                        transform.position = Vector3.Lerp(transform.position, targetPos, speedProg);
                        break;

                    case CAMERA_MODE.CHARACTER_FOCUS:

                        if (player != null)
                        {
                            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                            offset = (mouse - player.transform.position).normalized;

                            if (Vector2.Distance(mouse, player.transform.position) > 9)
                            {
                                offset_multiplier = Vector3.Distance(player.transform.position, mouse) * 0.95f;
                            }

                            offset_multiplier = Mathf.Clamp(offset_multiplier, 0, 50f);
                            offset *= offset_multiplier;
                            Vector3 vec = Vector3.Lerp(player.transform.position, transform.position, 0.6f);

                            vec.y = Mathf.RoundToInt(vec.y);
                            vec.x = Mathf.RoundToInt(vec.x);

                            /*
                            if (vec.x < orthGraphicSize)
                                vec.x = orthGraphicSize;
                            */

                            if (camWithMouseOffset)
                                transform.position = new Vector3(vec.x, vec.y, -10) + (Vector3)offset;
                            else
                                transform.position = new Vector3(vec.x, vec.y, -5);
                            /*
                            if (vec.x > mapSize.x * tileSize.x - orthGraphicSize)
                                vec.x = mapSize.x * tileSize.x - orthGraphicSize;

                            if (vec.y < orthGraphicSize / 2)
                                vec.y = orthGraphicSize / 2;
                            if (vec.y > mapSize.y * tileSize.y + orthGraphicSize / 2)
                                vec.y = mapSize.y * tileSize.y + orthGraphicSize / 2;
                            */
                        }
                        break;
                }

                if (camerashakeDel > 0)
                {
                    float cam_off_x = UnityEngine.Random.Range(-cameraoffset_X, cameraoffset_X);
                    float cam_off_y = UnityEngine.Random.Range(-cameraoffset_Y, cameraoffset_Y);

                    transform.position = new Vector3(transform.position.x + cam_off_x, transform.position.y + cam_off_y, -23);

                    camerashakeDel = camerashakeDel - Time.deltaTime;

                }
            }
        }

        public void CameraShake(float x, float y)
        {
            transform.position += new Vector3(x, y, 0);

            cameraMode = CAMERA_MODE.NONE;
        }

        public bool CameraLerp()
        {
            if (transform.position == targetPos)
            {
                //lerping = false;
                return true;
            }
            return false;
        }

        public void CameraLerpInit(Vector2 _startpos, Vector2 _targetpos)
        {
            startPos = _startpos + (Vector2)new Vector3(0, 0, -5);
            targetPos = _targetpos + (Vector2)new Vector3(0, 0, -5);
            speedProg = 0;
            cameraMode = CAMERA_MODE.LERPING;
        }
    }
}
