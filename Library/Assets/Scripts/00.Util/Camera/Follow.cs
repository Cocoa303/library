using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util.Camera
{
    public class Follow : MonoBehaviour
    {
        [SerializeField] Transform target;

        [SerializeField] float distance = 10.0f;    //== Distance between the camera and the target
        [SerializeField] float height = 5.0f;       //== Camera height
        [SerializeField] float rotationSmoothness = 2.0f;   //== Smoothness of camera rotation
        [SerializeField] float moveSmoothness = 2.0f;       //== Smoothness of camera movement

        //== Caching for call optimization
        private new Transform transform;
        private Vector3 offset;

        private void Start()
        {
            transform = GetComponent<Transform>();
            offset = new Vector3(0, height, -distance);

#if UNITY_EDITOR
            if (target == null)
            {
                Debug.LogError($"{gameObject.name}의 Component Util.Camera.Follow에 target이 설정되어있지 않습니다.");
            }
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (rotationSmoothness == 0 || moveSmoothness == 0)
            {
                Debug.LogError($"{gameObject.name}의 Component Util.Camera.Follow에 선형보간 값이 올바르지 않습니다. [ 0은 사용할 수 없습니다. ]");
                if (rotationSmoothness == 0) rotationSmoothness = 0.01f;
                if (moveSmoothness == 0) moveSmoothness = 0.01f;
            }
        }
#endif


        private void FixedUpdate()
        {
            if (!target) return;

            //== Operation target position
            Vector3 targetPosition = target.position + offset;

            //== Position Setting
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSmoothness * Time.deltaTime);

            //== Rotation Setting
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
        }
    }
}