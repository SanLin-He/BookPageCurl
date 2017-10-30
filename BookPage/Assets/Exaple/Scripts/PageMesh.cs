using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageMesh : MonoBehaviour
{

    public MeshFilter BookMf;
    private Mesh BookMesh;

    public PageTurnStage[] pageTurnStages =

    {
        new PageTurnStage(){ step =0.0f, rho = 0.0f, thetaAngle = 90.0f, apexValue =-1.875f },
        new PageTurnStage(){ step =50.0f, rho = 0.0f, thetaAngle =10.0f, apexValue = -1.875f },
        new PageTurnStage(){ step =70.0f, rho = 50.0f, thetaAngle =10.0f, apexValue = -1.875f },
        new PageTurnStage(){ step =90.0f, rho = 110.0f, thetaAngle =15.0f, apexValue = -1.875f },
        new PageTurnStage(){ step =100.0f, rho = 180.0f, thetaAngle =90.0f, apexValue = -1.875f },
    };

    private Vector3[] originVertexes;
    private Vector3[] deformVertexes;
    private float step = 0;
    #region unity
    // Use this for initialization
    void Start()
    {
        BookMesh = BookMf.mesh;
        originVertexes = BookMesh.vertices;
        deformVertexes = new Vector3[BookMesh.vertexCount];
    }

    // Update is called once per frame
    void Update()
    {
        if (step <= 100)
        {
            step += Time.deltaTime *10f;
            ReCaculateMesh(step);

        }
        else
        {
            Debug.Log("end");
        }
    }
    #endregion



    private void ReCaculateMesh(float step)//t范围 0% ~100%，翻页开始到翻页结束阶段
    {

        float theta = 0.0f; //取得theta（圆锥半角θ）
        float apex = 0.0f;//apex（圆锥顶点A，y轴分量值）
        float rho = 0.0f;//rho（ρ当前页面转顶线翻转的角度（0~180））
        GetThetaApexRho(step, out theta, out apex, out rho);


        //只修改顶点
        for (int i = 0; i < BookMesh.vertexCount; i++)
        {
            deformVertexes[i] = CurlTurn(originVertexes[i], theta, apex);
        }

        BookMesh.vertices = deformVertexes;
        BookMesh.RecalculateNormals();

        //transform.eulerAngles = new Vector3(rho, transform.eulerAngles.y, transform.eulerAngles.z);

    }

    /// <summary>
    ///  GetThetaApexRho内部计算是通过一个外部配置的阶段数组插值得到Theta、Apex、Rho的，这个阶段数组是一个经验值，需要通过不段实验来得到一个较为自然的结果
    /// </summary>
    /// <param name="step"></param>
    /// <param name="theta"></param>
    /// <param name="apex"></param>
    /// <param name="rho"></param>
    private void GetThetaApexRho(float step, out float theta, out float apex, out float rho)
    {
        theta = pageTurnStages[0].thetaAngle;
        apex = pageTurnStages[0].apexValue;
        rho = pageTurnStages[0].rho;
        for (int i = 0; i < pageTurnStages.Length - 2; i++)
        {
            if (step > pageTurnStages[i + 0].step && step <= pageTurnStages[i + 1].step)
            {
                float startStep = pageTurnStages[i + 0].step;
                float finalStep = pageTurnStages[i + 1].step;
                float t = step / (finalStep);
                Debug.Log("***t"+ t);
                float startTheta = pageTurnStages[i + 0].thetaAngle;
                float finalTheta = pageTurnStages[i + 1].thetaAngle;
                theta = Mathf.Lerp(startTheta, finalTheta, t);
                Debug.Log("***theta"+ theta);
                float startApex = pageTurnStages[i + 0].apexValue;
                float finalApex = pageTurnStages[i + 1].apexValue;
                apex = Mathf.Lerp(startApex, finalApex, t);
                Debug.Log("***Apex"+ apex);
                float startRho = pageTurnStages[i + 0].rho;
                float finalRho = pageTurnStages[i + 1].rho;
                rho = Mathf.Lerp(startRho, finalRho, t);
                return;
            }

        }



    }

    private Vector3 CurlTurn(Vector3 originP, float theta, float apex)
    {
        //坐标偏移世界坐标
        Vector3 p = transform.TransformPoint(new Vector3(originP.x, originP.y, originP.z));


        //float R = Mathf.Sqrt((p.x * p.x) + Mathf.Pow((p.y - apex), 2.0f));
        //float r = R * Mathf.Sin(theta);
        //float beta = Mathf.Asin(p.x / R) / Mathf.Sin(theta);
        //p.x = r * Mathf.Sin(beta);
        //p.y = (R + apex) - ((r * (1 - Mathf.Cos(beta))) * Mathf.Sin(theta));
        //p.z = -(r * (1 - Mathf.Cos(beta))) * Mathf.Cos(theta);

        //根据物体自身和unity的坐标系调整了算法
        float R = Mathf.Sqrt((p.x * p.x) + Mathf.Pow((p.z - apex), 2.0f));
        float r = R * Mathf.Sin(theta);
        float beta = Mathf.Asin(p.x / R) / Mathf.Sin(theta);
        p.x = r * Mathf.Sin(beta);
        p.z = (R + apex) - ((r * (1 - Mathf.Cos(beta))) * Mathf.Sin(theta));
        p.y = (r * (1 - Mathf.Cos(beta))) * Mathf.Cos(theta);

        p = transform.InverseTransformPoint(p);//物体坐标
        return new Vector3(p.x, p.y, p.z);

    }

    public class PageTurnStage
    {

        public float step;
        public float rho;
        public float thetaAngle;
        public float apexValue;
    }
}
