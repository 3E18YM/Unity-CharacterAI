
using CrazyMinnow.SALSA;
using System.Text.RegularExpressions;
using UnityEngine;


[ExecuteInEditMode]
public class Blendshape_Command : MonoBehaviour
{
    // Start is called before the first frame update


    Mesh skinnedMesh;
    string Prefix;
    int Bsindex;
    int lastBSindex;



    public SkinnedMeshRenderer skinnedMeshRenderer;


    public bool Connect_Component;
    public SkinnedMeshRenderer[] FaceObject;




    // Update is called once per frame
    void Awake()
    {


        skinnedMesh = skinnedMeshRenderer.sharedMesh;




        string pattern = @"\..*";
        Prefix = Regex.Replace(skinnedMesh.GetBlendShapeName(1), pattern, ".");



    }


    void LateUpdate()
    {

        if (Connect_Component)
        {
            foreach (var face in FaceObject)
            {


                for (int i = 0; i < face.sharedMesh.blendShapeCount; i++)
                {



                    string BsName = face.sharedMesh.GetBlendShapeName(i);
                    string pattern = @".*?\.";
                    string resultString = Regex.Replace(BsName, pattern, "");

                    int Bsindex = skinnedMesh.GetBlendShapeIndex(Prefix + resultString);

                    face.SetBlendShapeWeight(i, skinnedMeshRenderer.GetBlendShapeWeight(Bsindex));

                }

            }

        }



    }








}