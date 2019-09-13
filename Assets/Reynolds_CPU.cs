

//#define FLOAT                               //計算の精度、単精度はアンコメント、倍精度はコメントアウト

/*
 * Update()内で繰り返し計算をするメソッドを選択する
 * アンコメントの場合はCalcu_PerFrame_Optimized()
 * コメントアウトの場合はCalcu_PerFrame()
 * Calcu_PerFrame_Optimized()はほんの少し早い
 */
#define USE_OPTIMIZED




using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if FLOAT
using MyVar = System.Single;
#else
using MyVar = System.Double;
#endif

/// <summary>
/// 3次元レイノルズ方程式を計算するクラス
/// </summary>
public class Reynolds_CPU : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Toggle m_Toggle = null;


    private const uint meshX = 500, meshZ = 500;

    private ReynoldsFunc.Mesh mesh = null;


    // Use this for initialization
    void Start()
    {
        mesh = new ReynoldsFunc.Mesh(meshX, meshZ, 0.001f, 0.001f);
        mesh.CreateHeightArray((MyVar)0.00003, meshZ / 2);
        mesh.CreateDeltaXYArray();

#if USE_OPTIMIZED
        mesh.Initialize_Calcu_PerFrame_Optimized();
#else
        mesh.Initialize_Calcu_PerFrame();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Toggle.isOn)
        {

#if USE_OPTIMIZED
            mesh.Calcu_PerFrame_Optimized();
#else
        mesh.Calcu_PerFrame();
#endif
        }
    }
}


namespace ReynoldsFunc
{

    public class ConstValue
    {
        public const MyVar RIBANGLE_DEG = (MyVar)45.0;
        public const MyVar LIP_SHAFT_GAP = (MyVar)0.00001;
        public const MyVar LIPANGLE_DEG = (MyVar)20.0;

        /// <summary>
        /// 壁面の速度m/sec
        /// </summary>
        public const MyVar SLIDING_SPEED = (MyVar)(-5.6);
        /// <summary>
        /// 潤滑油Ps・s  URL:https://ja.wikipedia.org/wiki/%E7%B2%98%E5%BA%A6	
        /// </summary>
        public const MyVar VISCOSITY = (MyVar)0.058;

        /// <summary>
        /// 収束判定の閾値
        /// </summary>
        public const MyVar EP = (MyVar)0.000001;

        /// <summary>
        /// 最大ループ回数、Calcu()を使用する場合に有効
        /// </summary>
        public const int MAX_LOOPCOUNT = 1000000;
    }


    public class Mesh
    {
        //メンバ変数の宣言

        /// <summary>
        /// 格子点数 No. 0, 1, 2, 3, ・・・・, meshCountX-1のmeshCountX個を配置する
        /// </summary>
        private uint meshCountX;

        /// <summary>
        /// 格子点数 No. 0, 1, 2, 3, ・・・・, meshCountY-1のmeshCountZ個を配置する
        /// </summary>
        private uint meshCountZ;

        /// <summary>
        /// リブのオフセット(X方向の格子点番号)
        /// </summary>
        private uint sinewaveOffsetX;

        /// <summary>
        /// リブのオフセット(Z方向の格子点番号)
        /// </summary>
        private uint sinewaveOffsetZ;

        /// <summary>
        /// リップ油面角
        /// </summary>
        private MyVar lipAngle;

        /// <summary>
        /// リブの高さ
        /// </summary>
        private MyVar height;

        /// <summary>
        /// 計算領域のX方向距離
        /// </summary>
        private MyVar magX;

        /// <summary>
        /// 計算領域のZ方向距離
        /// </summary>
        private MyVar magZ;

        /// <summary>
        ///  圧力の二次元配列
        /// </summary>
        private MyVar[,] pressureArray = null;

        /// <summary>
        /// 繰り返し計算後の圧力の二次元配列
        /// </summary>
        private MyVar[,] formerPressureArray = null;

        /// <summary>
        /// 高さの二次元配列
        /// </summary>
        private MyVar[,] heightArray = null;

        /// <summary>
        /// 格子点のX座標
        /// </summary>
        private MyVar[] widthArray = null;

        /// <summary>
        /// 格子点のY座標
        /// </summary>
        private MyVar[] depthArray = null;

        MyVar[,] h1 = null;        //heigtArrayの1乗
        MyVar[,] h2 = null;        //heigtArrayの2乗
        MyVar[,] h3 = null;        //heigtArrayの3乗

        MyVar[,] dhdxArray = null;
        MyVar[,] dhdzArray = null;


        MyVar[,] AP = null;
        MyVar[,] AN = null;
        MyVar[,] AS = null;
        MyVar[,] AE = null;
        MyVar[,] AW = null;
        MyVar[,] SP = null;

        /// <summary>
        /// Calcu_PerFrame()またはInitialize_Calcu_PerFrame_Optimized()を実行する前にCreateHeightArray()を成功させないといけない。
        /// </summary>
        private bool isSucceedCreateHeightArray = false;

        /// <summary>
        /// Calcu_PerFrame()またはInitialize_Calcu_PerFrame_Optimized()を実行する前にCreateDeltaXYArray()を成功させないといけない。
        /// </summary>
        private bool isCreateDeltaXYArray = false;

        /// <summary>
        /// Calcu_PerFrame()を使用する場合はInitialize_Calcu_PerFrame()を成功させないといけない。 
        /// </summary>
        private bool isInitialize_Calcu_PerFrame = false;

        /// <summary>
        /// Calcu_PerFrame_Optimized()を使用する場合はisInitialize_Calcu_PerFrame_Optimized()を成功させないといけない。
        /// </summary>
        private bool isInitialize_Calcu_PerFrame_Optimized = false;

        /// <summary>
        /// trueの場合、Calcu_PerFrame()内での計算をスキップする
        /// </summary>
        private bool isLoopStop = false;


        /// <summary>
        /// X軸方向の格子点間距離の配列 例)deltaX[3] = widthArray[3] - widthArray[2];
        /// </summary>
        private MyVar[] deltaX = null;

        /// <summary>
        /// Z軸方向の格子点間距離の配列 例)deltaZ[3] = depthArray[3] - depthArray[2];
        /// </summary>
        private MyVar[] deltaZ = null;

        /// <summary>
        /// 繰り返し計算の実行回数
        /// </summary>
        private int loopCount = 0;


        /// <summary>
        /// 繰り返し計算を行い、結果が収束するまで呼び出し元へ制御を返しません。
        /// ※注意：収束するまで多量の計算が必要なのでフリーズしたようになります。
        /// </summary>
        public void Calcu()
        {
            if (!isSucceedCreateHeightArray || !isCreateDeltaXYArray) { return; }


            MyVar resudial = (MyVar)0.0;
            MyVar previousResudial = (MyVar)0.0;

            //while文の継続判定フラグ
            bool isLoopContinue = true;


            MyVar[,] h1 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの1乗
            MyVar[,] h2 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの2乗
            MyVar[,] h3 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの3乗

            MyVar tmpHeight = (MyVar)0;
            for (int j = 0; j < meshCountZ; j++)
            {
                for (int i = 0; i < meshCountX; i++)
                {
                    tmpHeight = heightArray[i, j];
                    h1[i, j] = tmpHeight;
                    h2[i, j] = tmpHeight * tmpHeight;
                    h3[i, j] = tmpHeight * tmpHeight * tmpHeight;
                }
            }


            MyVar dhdx = 0.0f;                     //微分
            MyVar dhdz = 0.0f;                     //微分

            MyVar hz = 0.0f;
            MyVar hx = 0.0f;

            MyVar WA = ConstValue.SLIDING_SPEED;
            MyVar ETA = ConstValue.VISCOSITY;

            /*分子分母のうち計算・メモリアクセスを簡略化できるものが有るので検討すること*/
            MyVar AP = (MyVar)0.0;
            MyVar AN = (MyVar)0.0;
            MyVar AS = (MyVar)0.0;
            MyVar AE = (MyVar)0.0;
            MyVar AW = (MyVar)0.0;
            MyVar SP = (MyVar)0.0;

            MyVar PN = (MyVar)0.0;
            MyVar PS = (MyVar)0.0;
            MyVar PE = (MyVar)0.0;
            MyVar PW = (MyVar)0.0;

            MyVar newPressure = (MyVar)0.0;

            MyVar maxPressure = (MyVar)0.0;

            MyVar[,] dhdxArray = new MyVar[meshCountX, meshCountZ];
            MyVar[,] dhdzArray = new MyVar[meshCountX, meshCountZ];

            for (int i = 1; i < (meshCountX - 1); i++)
            {
                for (int j = 1; j < (meshCountZ - 1); j++)
                {
                    //等間隔格子を想定
                    dhdxArray[i, j] = (heightArray[i + 1, j] - heightArray[i - 1, j]) / (deltaX[i + 1] + deltaX[i]);
                    dhdzArray[i, j] = (heightArray[i, j + 1] - heightArray[i, j - 1]) / (deltaZ[j + 1] + deltaZ[j]);
                }
            }

            for (int i = 1; i < (meshCountX - 1); i++)
            {
                for (int j = 1; j < (meshCountZ - 1); j++)
                {
                    formerPressureArray[i, j] = (MyVar)1.0;
                }
            }

            int loopCounter = 0;
            do
            {
                Debug.Log("Loop : " + loopCounter);

                previousResudial = (MyVar)0;

                maxPressure = (MyVar)0;

                for (int j = 1; j < (meshCountZ - 1); j++)
                {
                    for (int i = 1; i < (meshCountX - 1); i++)
                    {
                        MyVar height2 = h2[i, j];
                        MyVar height3 = h3[i, j];

                        dhdx = dhdxArray[i, j];
                        dhdz = dhdzArray[i, j];

                        hz = 3.0f * height2 * dhdz;
                        hx = 3.0f * height2 * dhdx;

                        WA = ConstValue.SLIDING_SPEED;              //壁面の速度
                        ETA = ConstValue.VISCOSITY;

                        PN = formerPressureArray[i, j + 1];
                        PS = formerPressureArray[i, j - 1];
                        PE = formerPressureArray[i + 1, j];
                        PW = formerPressureArray[i - 1, j];

                        /*分子分母のうち計算・メモリアクセスを簡略化できるものが有るので検討すること*/
                        AP = 2.0f * (deltaX[i + 1] + deltaX[i]) / (deltaX[i + 1] * deltaX[i] * (deltaX[i + 1] + deltaX[i])) * height3 + 2.0f * (deltaZ[j + 1] + deltaZ[j]) / (deltaZ[j + 1] * deltaZ[j] * (deltaZ[j + 1] + deltaZ[j])) * height3;
                        AN = 2.0f * height3 / (deltaZ[j + 1] * (deltaZ[j + 1] + deltaZ[j])) + hz / (deltaZ[j + 1] + deltaZ[j]);
                        AS = 2.0f * height3 / (deltaZ[j] * (deltaZ[j + 1] + deltaZ[j])) - hz / (deltaZ[j + 1] + deltaZ[j]);
                        AE = 2.0f * height3 / (deltaX[i + 1] * (deltaX[i + 1] + deltaX[i])) + hx / (deltaX[i + 1] + deltaX[i]);
                        AW = 2.0f * height3 / (deltaX[i] * (deltaX[i + 1] + deltaX[i])) - hx / (deltaX[i + 1] + deltaX[i]);

                        SP = -6.0f * WA * ETA * dhdz;

                        newPressure = (AN * PN + AS * PS + AE * PE + AW * PW + SP) / (AP);

                        resudial = System.Math.Abs((newPressure - formerPressureArray[i, j]) / newPressure);

                        if (resudial > previousResudial) { previousResudial = resudial; }

                        if (maxPressure < newPressure) { maxPressure = newPressure; }

                        pressureArray[i, j] = newPressure;

                        if (previousResudial > ConstValue.EP) { isLoopContinue = true; }
                        else { isLoopContinue = false; }
                    }


                }

                Debug.Log("resudial = " + resudial);

                System.Array.Copy(pressureArray, formerPressureArray, pressureArray.Length);

                loopCounter++;

                if (loopCounter >= ConstValue.MAX_LOOPCOUNT)
                {
                    isLoopContinue = false;
                }

            } while (isLoopContinue);

        }


        /// <summary>
        /// Update()関数内で計算を実行する前に初期化する。CPUで計算する。
        /// </summary>
        public void Initialize_Calcu_PerFrame()
        {
            if (!isSucceedCreateHeightArray || !isCreateDeltaXYArray) { return; }

            //---------------------------------------------------------------------------------
            h1 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの1乗
            h2 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの2乗
            h3 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの3乗

            MyVar tmpHeight = (MyVar)0;
            for (int j = 0; j < meshCountZ; j++)
            {
                for (int i = 0; i < meshCountX; i++)
                {
                    tmpHeight = heightArray[i, j];
                    h1[i, j] = tmpHeight;
                    h2[i, j] = tmpHeight * tmpHeight;
                    h3[i, j] = tmpHeight * tmpHeight * tmpHeight;
                }
            }
            //---------------------------------------------------------------------------------

            dhdxArray = new MyVar[meshCountX, meshCountZ];
            dhdzArray = new MyVar[meshCountX, meshCountZ];

            for (int i = 1; i < (meshCountX - 1); i++)
            {
                for (int j = 1; j < (meshCountZ - 1); j++)
                {
                    //等間隔格子を想定
                    dhdxArray[i, j] = (heightArray[i + 1, j] - heightArray[i - 1, j]) / (deltaX[i + 1] + deltaX[i]);
                    dhdzArray[i, j] = (heightArray[i, j + 1] - heightArray[i, j - 1]) / (deltaZ[j + 1] + deltaZ[j]);
                }
            }
            //---------------------------------------------------------------------------------

            formerPressureArray = new MyVar[meshCountX, meshCountZ];
            pressureArray = new MyVar[meshCountX, meshCountZ];

            for (int j = 0; j < meshCountZ; j++)
            {
                for (int i = 0; i < meshCountX; i++)
                {
                    formerPressureArray[i, j] = (MyVar)0;
                    pressureArray[i, j] = (MyVar)0;
                }
            }

            for (int i = 1; i < (meshCountX - 1); i++)
            {
                for (int j = 1; j < (meshCountZ - 1); j++)
                {
                    formerPressureArray[i, j] = (MyVar)1.0;                 //計算を行う計算格子点の前回の圧力に仮の値を代入
                }
            }
            //---------------------------------------------------------------------------------

            isInitialize_Calcu_PerFrame = true;

        }

        /// <summary>
        /// Update()関数内で計算を実行する。Initialize_Calcu_PerFrame()を先に成功させないといけない。
        /// </summary>
        public void Calcu_PerFrame()
        {
            if (!isSucceedCreateHeightArray || !isCreateDeltaXYArray || !isInitialize_Calcu_PerFrame) { return; }

            if (isLoopStop) { return; }

            loopCount++;

            MyVar resudial = (MyVar)0.0;
            MyVar previousResudial = (MyVar)0.0;

            MyVar dhdx = 0.0f;                     //微分
            MyVar dhdz = 0.0f;                     //微分

            MyVar hz = 0.0f;
            MyVar hx = 0.0f;

            MyVar WA = ConstValue.SLIDING_SPEED;
            MyVar ETA = ConstValue.VISCOSITY;

            /*分子分母のうち計算・メモリアクセスを簡略化できるものが有るので検討すること*/
            MyVar AP = (MyVar)0.0;
            MyVar AN = (MyVar)0.0;
            MyVar AS = (MyVar)0.0;
            MyVar AE = (MyVar)0.0;
            MyVar AW = (MyVar)0.0;
            MyVar SP = (MyVar)0.0;

            MyVar PN = (MyVar)0.0;
            MyVar PS = (MyVar)0.0;
            MyVar PE = (MyVar)0.0;
            MyVar PW = (MyVar)0.0;

            MyVar newPressure = (MyVar)0.0;

            MyVar maxPressure = (MyVar)0.0;


            previousResudial = (MyVar)0;

            maxPressure = (MyVar)0;

            for (int j = 1; j < (meshCountZ - 1); j++)
            {
                for (int i = 1; i < (meshCountX - 1); i++)
                {
                    MyVar height2 = h2[i, j];
                    MyVar height3 = h3[i, j];

                    dhdx = dhdxArray[i, j];
                    dhdz = dhdzArray[i, j];

                    hz = 3.0f * height2 * dhdz;
                    hx = 3.0f * height2 * dhdx;

                    WA = ConstValue.SLIDING_SPEED;              //壁面の速度
                    ETA = ConstValue.VISCOSITY;

                    PN = formerPressureArray[i, j + 1];
                    PS = formerPressureArray[i, j - 1];
                    PE = formerPressureArray[i + 1, j];
                    PW = formerPressureArray[i - 1, j];

                    /*分子分母のうち計算・メモリアクセスを簡略化できるものが有るので検討すること*/
                    AP = 2.0f * (deltaX[i + 1] + deltaX[i]) / (deltaX[i + 1] * deltaX[i] * (deltaX[i + 1] + deltaX[i])) * height3 + 2.0f * (deltaZ[j + 1] + deltaZ[j]) / (deltaZ[j + 1] * deltaZ[j] * (deltaZ[j + 1] + deltaZ[j])) * height3;
                    AN = 2.0f * height3 / (deltaZ[j + 1] * (deltaZ[j + 1] + deltaZ[j])) + hz / (deltaZ[j + 1] + deltaZ[j]);
                    AS = 2.0f * height3 / (deltaZ[j] * (deltaZ[j + 1] + deltaZ[j])) - hz / (deltaZ[j + 1] + deltaZ[j]);
                    AE = 2.0f * height3 / (deltaX[i + 1] * (deltaX[i + 1] + deltaX[i])) + hx / (deltaX[i + 1] + deltaX[i]);
                    AW = 2.0f * height3 / (deltaX[i] * (deltaX[i + 1] + deltaX[i])) - hx / (deltaX[i + 1] + deltaX[i]);

                    //				AP = 2.0f * (1.0f / deltaX[i + 1] + 1.0f / deltaX[i]) / (deltaX[i + 1] + deltaX[i]) * height3 + 2.0f * (1.0f / deltaZ[j + 1] + 1.0f / deltaZ[j]) / (deltaZ[j + 1] + deltaZ[j]) * height3;
                    //				AN = 2.0f *   height3 / (deltaZ[j + 1] * (deltaZ[j + 1] + deltaZ[j])) + hz / (deltaZ[j + 1] + deltaZ[j]);
                    //				AS = 2.0f *   height3 / (deltaZ[j] * (deltaZ[j + 1] + deltaZ[j])) - hz / (deltaZ[j + 1] + deltaZ[j]);
                    //				AE = 2.0f *   height3 / (deltaX[i + 1] * (deltaX[i + 1] + deltaX[i])) + hx / (deltaX[i + 1] + deltaX[i]);
                    //				AW = 2.0f *   height3 / (deltaX[i] * (deltaX[i + 1] + deltaX[i])) - hx / (deltaX[i + 1] + deltaX[i]);

                    SP = -6.0f * WA * ETA * dhdz;
                    //				SP = + 6.0f * WA * ETA * dhdz;

                    newPressure = (AN * PN + AS * PS + AE * PE + AW * PW + SP) / (AP);

                    resudial = System.Math.Abs((newPressure - formerPressureArray[i, j]) / newPressure);

                    if (resudial > previousResudial) { previousResudial = resudial; }

                    if (maxPressure < newPressure) { maxPressure = newPressure; }

                    pressureArray[i, j] = newPressure;

                }
            }

            if (previousResudial < ConstValue.EP)
            {
                isLoopStop = true;                      //収束した場合
            }
            else
            {
                isLoopStop = false;                     //収束していない場合
            }

            string str = System.String.Format("loop = {2} : previousResudial = {0:E5} : maxPressure = {1:E5}", previousResudial, maxPressure, loopCount);
            Debug.Log(str);

            System.Array.Copy(pressureArray, formerPressureArray, pressureArray.Length);

        }

        /// <summary>
        /// Calcu_PerFrame_Optimized()を実行する場合は先に呼ばないといけない。
        /// </summary>
        public void Initialize_Calcu_PerFrame_Optimized()
        {
            if (!isSucceedCreateHeightArray || !isCreateDeltaXYArray) { return; }

            //---------------------------------------------------------------------------------
            h1 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの1乗
            h2 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの2乗
            h3 = new MyVar[meshCountX, meshCountZ];        //heigtArrayの3乗

            MyVar tmpHeight = (MyVar)0;
            for (int j = 0; j < meshCountZ; j++)
            {
                for (int i = 0; i < meshCountX; i++)
                {
                    tmpHeight = heightArray[i, j];
                    h1[i, j] = tmpHeight;
                    h2[i, j] = tmpHeight * tmpHeight;
                    h3[i, j] = tmpHeight * tmpHeight * tmpHeight;
                }
            }
            //---------------------------------------------------------------------------------

            dhdxArray = new MyVar[meshCountX, meshCountZ];
            dhdzArray = new MyVar[meshCountX, meshCountZ];

            for (int i = 1; i < (meshCountX - 1); i++)
            {
                for (int j = 1; j < (meshCountZ - 1); j++)
                {
                    //等間隔格子を想定
                    dhdxArray[i, j] = (heightArray[i + 1, j] - heightArray[i - 1, j]) / (deltaX[i + 1] + deltaX[i]);
                    dhdzArray[i, j] = (heightArray[i, j + 1] - heightArray[i, j - 1]) / (deltaZ[j + 1] + deltaZ[j]);
                }
            }
            //---------------------------------------------------------------------------------

            formerPressureArray = new MyVar[meshCountX, meshCountZ];
            pressureArray = new MyVar[meshCountX, meshCountZ];

            for (int j = 0; j < meshCountZ; j++)
            {
                for (int i = 0; i < meshCountX; i++)
                {
                    formerPressureArray[i, j] = (MyVar)0;
                    pressureArray[i, j] = (MyVar)0;
                }
            }

            for (int i = 1; i < (meshCountX - 1); i++)
            {
                for (int j = 1; j < (meshCountZ - 1); j++)
                {
                    formerPressureArray[i, j] = (MyVar)1.0;                 //計算を行う計算格子点の前回の圧力に仮の値を代入
                }
            }
            //---------------------------------------------------------------------------------

            AP = new MyVar[meshCountX, meshCountZ];
            AN = new MyVar[meshCountX, meshCountZ];
            AS = new MyVar[meshCountX, meshCountZ];
            AE = new MyVar[meshCountX, meshCountZ];
            AW = new MyVar[meshCountX, meshCountZ];
            SP = new MyVar[meshCountX, meshCountZ];

            for (int j = 1; j < (meshCountZ - 1); j++)
            {
                for (int i = 1; i < (meshCountX - 1); i++)
                {
                    MyVar height2 = h2[i, j];
                    MyVar height3 = h3[i, j];

                    MyVar dhdx = dhdxArray[i, j];
                    MyVar dhdz = dhdzArray[i, j];

                    MyVar hz = 3.0f * height2 * dhdz;
                    MyVar hx = 3.0f * height2 * dhdx;

                    AP[i, j] = 2.0f * (deltaX[i + 1] + deltaX[i]) / (deltaX[i + 1] * deltaX[i] * (deltaX[i + 1] + deltaX[i])) * height3 + 2.0f * (deltaZ[j + 1] + deltaZ[j]) / (deltaZ[j + 1] * deltaZ[j] * (deltaZ[j + 1] + deltaZ[j])) * height3;
                    AN[i, j] = 2.0f * height3 / (deltaZ[j + 1] * (deltaZ[j + 1] + deltaZ[j])) + hz / (deltaZ[j + 1] + deltaZ[j]);
                    AS[i, j] = 2.0f * height3 / (deltaZ[j] * (deltaZ[j + 1] + deltaZ[j])) - hz / (deltaZ[j + 1] + deltaZ[j]);
                    AE[i, j] = 2.0f * height3 / (deltaX[i + 1] * (deltaX[i + 1] + deltaX[i])) + hx / (deltaX[i + 1] + deltaX[i]);
                    AW[i, j] = 2.0f * height3 / (deltaX[i] * (deltaX[i + 1] + deltaX[i])) - hx / (deltaX[i + 1] + deltaX[i]);

                    MyVar WA = ConstValue.SLIDING_SPEED;              //壁面の速度
                    MyVar ETA = ConstValue.VISCOSITY;

                    SP[i, j] = -6.0f * WA * ETA * dhdz;
                }
            }

            isInitialize_Calcu_PerFrame_Optimized = true;

        }

        /// <summary>
        /// Update()関数内で計算を実行する。Initialize_Calcu_PerFrame_Optimized()を先に成功させないといけない。
        /// </summary>
        public void Calcu_PerFrame_Optimized()
        {
            if (!isSucceedCreateHeightArray || !isCreateDeltaXYArray || !isInitialize_Calcu_PerFrame_Optimized) { return; }
            if (isLoopStop) { return; }

            loopCount++;

            MyVar maxPressure = (MyVar)0;
            MyVar currentResudial = (MyVar)0;           //現在の残差
            MyVar tmpResudial = (MyVar)0;               //直前までの残差

            MyVar newPressure;
            MyVar PN, PS, PE, PW;
            for (int j = 1; j < (meshCountZ - 1); j++)
            {
                for (int i = 1; i < (meshCountX - 1); i++)
                {
                    PN = formerPressureArray[i, j + 1];
                    PS = formerPressureArray[i, j - 1];
                    PE = formerPressureArray[i + 1, j];
                    PW = formerPressureArray[i - 1, j];

                    newPressure = (AN[i, j] * PN + AS[i, j] * PS + AE[i, j] * PE + AW[i, j] * PW + SP[i, j]) / (AP[i, j]);

                    tmpResudial = System.Math.Abs((newPressure - formerPressureArray[i, j]) / newPressure);         //現在の計算格子の計算結果の残差得る

                    currentResudial = currentResudial > tmpResudial ? currentResudial : tmpResudial;                //残差を更新

                    pressureArray[i, j] = newPressure;                                                              //圧力を更新

                    maxPressure = maxPressure >= newPressure ? maxPressure : newPressure;                           //最高圧力を更新
                }
            }

            string str = System.String.Format("loop = {2} : previousResudial = {0:E5} : maxPressure = {1:E5}", currentResudial, maxPressure, loopCount);
            Debug.Log(str);

            System.Array.Copy(pressureArray, formerPressureArray, pressureArray.Length);

            if (currentResudial < ConstValue.EP)        //残差が十分に小さくなったか?
            {
                isLoopStop = true;                      //収束していないので計算を行う
            }
            else
            {
                isLoopStop = false;                     //収束したので計算を行わない
            }

        }

        /// <summary>
        /// このデフォルトコンストラクタは使わないでください
        /// </summary>
        private Mesh() { }//デフォルトコンストラクタは何もしない

        /// <summary>
        /// このコンストラクタを必ず使用してください
        /// </summary>
        /// <param name="x">X方向の格子点数</param>
        /// <param name="z">Z方向の格子点数</param>
        /// <param name="magx">計算領域のX方向距離、内部で絶対値に変換する。</param>
        /// <param name="magz">計算領域のZ方向距離、内部で絶対値に変換する。</param>
        public Mesh(uint x, uint z, MyVar magx, MyVar magz)
        {
            if (x == 0)
            {
                x = 1;
            }
            if (z == 0)
            {
                z = 1;
            }

            lipAngle = ConstValue.RIBANGLE_DEG;

            meshCountX = x + 1;
            meshCountZ = z + 1;

            magX = (MyVar)System.Math.Abs(magx);
            magZ = (MyVar)System.Math.Abs(magz);

            widthArray = new MyVar[meshCountX];
            for (int i = 0; i < meshCountX; i++)
            {
                widthArray[i] = magX / (MyVar)(meshCountX - 1) * (MyVar)i;
            }

            depthArray = new MyVar[meshCountZ];
            for (int j = 0; j < meshCountZ; j++)
            {
                depthArray[j] = magZ / (MyVar)(meshCountZ - 1) * (MyVar)j;
            }

            heightArray = new MyVar[meshCountX, meshCountZ];
            for (int j = 0; j < meshCountZ; j++)
            {
                for (int i = 0; i < meshCountX; i++)
                {
                    heightArray[i, j] = (MyVar)0;           //ゼロで初期化
                }
            }


        }

        /// <summary>
        /// 計算格子の高さをセットする
        /// </summary>
        /// <param name="h"></param>
        /// <param name="sinewaveoffsetZ"></param>
        /// <param name="swaveAngleFromAxeZ"></param>
        /// <param name="gap"></param>
        public void CreateHeightArray(MyVar h, uint sinewaveoffsetZ, MyVar swaveAngleFromAxeZ = ConstValue.RIBANGLE_DEG, MyVar gap = ConstValue.LIP_SHAFT_GAP)
        {
            if (h <= 0.0f) { h = (MyVar)1.0; }
            height = h;

            if (sinewaveoffsetZ >= meshCountZ) { sinewaveoffsetZ = 0; }
            sinewaveOffsetZ = sinewaveoffsetZ;

            sinewaveOffsetX = 0;


            for (uint i = 0; i < meshCountX; i++)
            {
                for (uint j = 0; j < meshCountZ; j++)
                {
                    MyVar xx = widthArray[i];
                    MyVar zz = depthArray[j];

                    MyVar offsetx = magX / (MyVar)meshCountX * (MyVar)sinewaveOffsetX;
                    MyVar offsetz = magZ / (MyVar)meshCountZ * (MyVar)sinewaveOffsetZ;

                    VECTOR tmpVect = new VECTOR(xx, 0, zz);

                    VECTOR tmpVectOffset = new VECTOR(offsetx, 0.0f, -offsetz);

                    tmpVect = tmpVect + tmpVectOffset;

                    matrix rotateMat = new matrix();
                    rotateMat.rotateY(swaveAngleFromAxeZ);

                    tmpVect = tmpVect * rotateMat;

                    MyVar sineXWidth = height;
                    MyVar sineXMax = (MyVar)System.Math.Abs(sineXWidth);

                    MyVar pi = (MyVar)3.1415926;

                    if (tmpVect.X <= sineXMax && tmpVect.X >= -sineXMax)
                    {
                        heightArray[i, j] = -height - height * (MyVar)System.Math.Cos((double)(pi / sineXMax * tmpVect.X));
                    }
                    else
                    {
                        heightArray[i, j] = (MyVar)0;
                    }

                    //リップの大気側油面角LIPANGLE_DEG分だけ傾ける
                    heightArray[i, j] += widthArray[i] * (MyVar)System.Math.Tan((double)pi / 180.0 * (double)lipAngle);

                    if (heightArray[i, j] < (MyVar)0) { heightArray[i, j] = (MyVar)0; }

                    heightArray[i, j] += gap;
                }
            }
            isSucceedCreateHeightArray = true;
        }

        /// <summary>
        /// X、Y軸上の格子点間距離を格納した配列deltaX、deltaZを作成する
        /// </summary>
        public void CreateDeltaXYArray()
        {
            bool checkParam = true;
            if (widthArray == null) { checkParam = false; ; }
            if (depthArray == null) { checkParam = false; ; }

            if (widthArray.Length <= 0) { checkParam = false; ; }
            if (depthArray.Length <= 0) { checkParam = false; ; }

            if (!checkParam)
            {
                isCreateDeltaXYArray = false;
                return;
            }

            deltaX = new MyVar[meshCountX];
            deltaZ = new MyVar[meshCountZ];


            for (int i = 1; i < meshCountX; i++)
            {
                deltaX[i] = (widthArray[i] - widthArray[i - 1]);
                Debug.Log("deltaX : " + deltaX[i]);
            }

            for (int j = 1; j < meshCountZ; j++)
            {
                deltaZ[j] = (depthArray[j] - depthArray[j - 1]);

                Debug.Log("deltaZ : " + deltaZ[j]);
            }

            isCreateDeltaXYArray = true;

        }



        public class matrix
        {
            /// <summary>
            /// matrixを初期値を指定せず初期化する。要素は全て0で初期化される。
            /// </summary>
            public matrix()
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.m[i, j] = (MyVar)0;
                    }
                }
            }

            /// <summary>
            /// matrixの初期値を指定して初期化する。
            /// </summary>
            /// <param name="v1">maxrixの1行目の要素に値を渡す。[0,0] = v1.X, [0,1] = v1.Y, [0,2] = v1.Z</param>
            /// <param name="v2">maxrixの2行目の要素に値を渡す。[1,0] = v2.X, [1,1] = v2.Y, [1,2] = v2.Z</param>
            /// <param name="v3">maxrixの3行目の要素に値を渡す。[2,0] = v3.X, [2,1] = v3.Y, [2,2] = v3.Z</param>
            public matrix(VECTOR v1, VECTOR v2, VECTOR v3)
            {
                this.m[0, 0] = v1.X;
                this.m[0, 1] = v1.Y;
                this.m[0, 2] = v1.Z;

                this.m[1, 0] = v2.X;
                this.m[1, 1] = v2.Y;
                this.m[1, 2] = v2.Z;

                this.m[2, 0] = v3.X;
                this.m[2, 1] = v3.Y;
                this.m[2, 2] = v3.Z;
            }
            /// <summary>
            /// 
            /// </summary>
            public MyVar[,] m = new MyVar[3, 3];

            /// <summary>
            /// このmatrixをX軸周りでdeg°回転する回転行列にする。
            /// </summary>
            /// <param name="deg">X軸周りの回転角度</param>
            public void rotateX(MyVar deg)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.m[i, j] = (MyVar)0;
                    }
                }

                MyVar pi = (MyVar)3.1415926;
                MyVar rad = pi / (MyVar)180 * deg;
                MyVar cosTheta = (MyVar)System.Math.Cos((double)rad);
                MyVar sinTheta = (MyVar)System.Math.Sign((double)rad);

                this.m[0, 0] = (MyVar)1.0;

                this.m[1, 1] = (MyVar)cosTheta;
                this.m[1, 2] = (MyVar)(-sinTheta);

                this.m[2, 1] = (MyVar)sinTheta;
                this.m[2, 2] = (MyVar)cosTheta;
            }
            /// <summary>
            /// このmatrixをY軸周りでdeg°回転する回転行列にする。
            /// </summary>
            /// <param name="deg">Y軸周りの回転角度</param>
            public void rotateY(MyVar deg)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.m[i, j] = (MyVar)0;
                    }
                }

                MyVar pi = (MyVar)3.1415926;
                MyVar rad = pi / (MyVar)180 * deg;
                MyVar cosTheta = (MyVar)System.Math.Cos((double)rad);
                MyVar sinTheta = (MyVar)System.Math.Sign((double)rad);

                this.m[0, 0] = (MyVar)cosTheta;
                this.m[0, 2] = (MyVar)sinTheta;

                this.m[1, 1] = (MyVar)1.0;

                this.m[2, 0] = (MyVar)(-sinTheta);
                this.m[2, 2] = (MyVar)cosTheta;
            }
            /// <summary>
            /// このmatrixをZ軸軸周りでdeg°回転する回転行列にする。
            /// </summary>
            /// <param name="deg">Z軸周りの回転角度</param>
            public void rotateZ(MyVar deg)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.m[i, j] = (MyVar)0;
                    }
                }

                MyVar pi = (MyVar)3.1415926;
                MyVar rad = pi / (MyVar)180 * deg;
                MyVar cosTheta = (MyVar)System.Math.Cos((double)rad);
                MyVar sinTheta = (MyVar)System.Math.Sign((double)rad);


                this.m[0, 0] = (MyVar)cosTheta;
                this.m[1, 1] = (MyVar)cosTheta;

                this.m[0, 1] = (MyVar)(-sinTheta);
                this.m[1, 0] = (MyVar)sinTheta;

                this.m[2, 2] = (MyVar)1.0;
            }
            public static matrix operator *(matrix m1, matrix m2)
            {
                MyVar _11, _12, _13;
                MyVar _21, _22, _23;
                MyVar _31, _32, _33;

                _11 = m1.m[0, 0];
                _12 = m1.m[0, 1];
                _13 = m1.m[0, 2];

                _21 = m1.m[1, 0];
                _22 = m1.m[1, 1];
                _23 = m1.m[1, 2];

                _31 = m1.m[2, 0];
                _32 = m1.m[2, 1];
                _33 = m1.m[2, 2];

                MyVar _arg11, _arg12, _arg13;
                MyVar _arg21, _arg22, _arg23;
                MyVar _arg31, _arg32, _arg33;

                _arg11 = m2.m[0, 0];
                _arg12 = m2.m[0, 1];
                _arg13 = m2.m[0, 2];

                _arg21 = m2.m[1, 0];
                _arg22 = m2.m[1, 1];
                _arg23 = m2.m[1, 2];

                _arg31 = m2.m[2, 0];
                _arg32 = m2.m[2, 1];
                _arg33 = m2.m[2, 2];

                MyVar _rslt11, _rslt12, _rslt13;
                MyVar _rslt21, _rslt22, _rslt23;
                MyVar _rslt31, _rslt32, _rslt33;

                _rslt11 = _11 * _arg11 + _12 * _arg21 + _13 * _arg31;
                _rslt12 = _11 * _arg12 + _12 * _arg22 + _13 * _arg32;
                _rslt13 = _11 * _arg13 + _12 * _arg23 + _13 * _arg33;

                _rslt21 = _21 * _arg11 + _22 * _arg21 + _23 * _arg31;
                _rslt22 = _21 * _arg12 + _22 * _arg22 + _23 * _arg32;
                _rslt23 = _21 * _arg13 + _22 * _arg23 + _23 * _arg33;

                _rslt31 = _31 * _arg11 + _32 * _arg31 + _33 * _arg31;
                _rslt32 = _31 * _arg12 + _32 * _arg32 + _33 * _arg32;
                _rslt33 = _31 * _arg13 + _32 * _arg33 + _33 * _arg33;

                VECTOR v1 = new VECTOR(_rslt11, _rslt12, _rslt13);
                VECTOR v2 = new VECTOR(_rslt21, _rslt22, _rslt23);
                VECTOR v3 = new VECTOR(_rslt31, _rslt32, _rslt33);

                return new matrix(v1, v2, v3);
            }
            //public static matrix operator=(){}代入演算子のオペレータはオーバーロードできないらしい //参考URL:https://ufcpp.net/study/csharp/oo_operator.html
        };

        public class VECTOR
        {
            /// <summary>
            /// 要素を全て0で初期化する。
            /// </summary>
            public VECTOR() { x = y = z = (MyVar)0; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            public VECTOR(MyVar x, MyVar y, MyVar z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            private MyVar x, y, z;

            public MyVar X
            {
                set { x = value; }
                get { return x; }
            }

            public MyVar Y
            {
                set { y = value; }
                get { return y; }
            }

            public MyVar Z
            {
                set { z = value; }
                get { return z; }
            }


            public void SetValue(MyVar xx, MyVar yy, MyVar zz)
            {
                this.x = xx;
                this.y = yy;
                this.z = zz;
            }
            /// <summary>
            /// VECTOR vとmatrix mの積を格納したインスタンスを返す。
            /// </summary>
            /// <param name="v"></param>
            /// <param name="m"></param>
            /// <returns></returns>
            static public VECTOR operator *(VECTOR v, matrix m)
            {
                MyVar _1, _2, _3;

                _1 = v.X * m.m[0, 0] + v.Y * m.m[1, 0] + v.Z * m.m[2, 0];
                _2 = v.X * m.m[0, 1] + v.Y * m.m[1, 1] + v.Z * m.m[2, 1];
                _3 = v.X * m.m[0, 2] + v.Y * m.m[1, 2] + v.Z * m.m[2, 2];

                return new VECTOR(_1, _2, _3);
            }
            /// <summary>
            /// VECTOR v1とv2の和を格納したVECTORのインスタンスを返す。
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <returns></returns>
            static public VECTOR operator +(VECTOR v1, VECTOR v2)
            {
                MyVar _1, _2, _3;

                _1 = v1.X + v2.X;
                _2 = v1.Y + v2.Y;
                _3 = v1.Z + v2.Z;

                return new VECTOR(_1, _2, _3);
            }
        };

    }
}