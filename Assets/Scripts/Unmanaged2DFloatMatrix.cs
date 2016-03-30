using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public unsafe class Unmanaged2DFloatMatrix
{
    private float* Ptr;

    public int SizeX
    {
        get;
        private set;
    }

    public int SizeY
    {
        get;
        private set;
    }

    public Unmanaged2DFloatMatrix(int SizeX, int SizeY)
    {
        this.SizeX = SizeX;
        this.SizeY = SizeY;

        Ptr = (float*)Marshal.AllocHGlobal(sizeof(float) * SizeX * SizeY);

        Clear();
    }

    ~Unmanaged2DFloatMatrix()
    {
        Marshal.FreeHGlobal((IntPtr)Ptr);
    }

    public float this[int XIndex, int YIndex]
    {
        get
        {
            if (XIndex >= 0 && XIndex < SizeX)
            {
                if (YIndex >= 0 && YIndex < SizeY)
                {
                    return *(Ptr + (XIndex * SizeY + YIndex));
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
            else
                throw new ArgumentOutOfRangeException();
        }

        set
        {
            if (XIndex >= 0 && XIndex < SizeX)
            {
                if (YIndex >= 0 && YIndex < SizeY)
                {
                    *(Ptr + (XIndex * SizeY + YIndex)) = value;
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
            else
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < SizeX * SizeY; i++)
        {
            *(Ptr + i) = 0;
        }
    }
}
