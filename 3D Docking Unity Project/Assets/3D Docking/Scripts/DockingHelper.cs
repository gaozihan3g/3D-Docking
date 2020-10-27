using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DockingHelper
{

    [System.Flags]
    public enum ManipulationType
    {
        Translation = 4,
        Rotation = 2,
        //Scale = 1,
    }

    public static Quaternion GetDeltaQuaternion(Quaternion from, Quaternion to)
    {
        Quaternion d = to * Quaternion.Inverse(from);
        return d;
    }

    public static float Map(float v, float fmin, float fmax, float tmin, float tmax, bool clamp = false)
    {
        float fd = fmax - fmin;
        float t = (v - fmin) / fd;
        float td = tmax - tmin;
        float r = tmin + t * td;
        if (clamp)
            return Mathf.Clamp(r, tmin, tmax);
        return r;
    }


    /// <summary>
    /// theta, r, h
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public static Vector3 CartesianToCylindrical(Vector3 pos, Vector3 origin)
    {
        Vector3 p = pos - origin;

        float R = Mathf.Sqrt(p.x * p.x + p.z * p.z);
        float H = p.y;
        float theta = Mathf.Atan2(p.z, p.x);

        Vector3 result = new Vector3(theta, R, H);

        return result;
    }


    public static Vector3 CylindricalToCartesian(Vector3 cld, Vector3 origin)
    {
        float theta = cld.x;
        float R = cld.y;
        float H = cld.z;

        float x = R * Mathf.Cos(theta);
        float y = H;
        float z = R * Mathf.Sin(theta);

        Vector3 result = new Vector3(x, y, z) + origin;

        return result;
    }


    /// <summary>
    /// r, polar theta, azimuth phi
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="origin"></param>
    /// <returns>r, polar-theta, azimuth-phi</returns>
    public static Vector3 CartesianToShpherical(Vector3 pos, Vector3 origin)
    {
        Vector3 p = pos - origin;

        float R = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
        float theta = Mathf.Atan2(Mathf.Sqrt(p.x * p.x + p.z * p.z), p.y);
        float phi = Mathf.Atan2(p.z, p.x);

        Vector3 result = new Vector3(R, theta, phi);

        return result;
    }

    public static Vector3 ShphericalToCartesian(Vector3 sphr, Vector3 origin)
    {
        float R = sphr.x;
        float theta = sphr.y;
        float phi = sphr.z;

        float x = R * Mathf.Sin(theta) * Mathf.Cos(phi);
        float y = R * Mathf.Cos(theta);
        float z = R * Mathf.Sin(theta) * Mathf.Sin(phi);

        Vector3 result = new Vector3(x, y, z) + origin;

        return result;
    }



}
