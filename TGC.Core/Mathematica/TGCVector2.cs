﻿using Microsoft.DirectX;
using System;
using TGC.Core.Utils;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a vector in two-dimensional (2-D) space.
    /// </summary>
    public struct TGCVector2
    {

        private static TGCVector2 ZERO = new TGCVector2(0f, 0f);
        private static TGCVector2 ONE = new TGCVector2(1f, 1f);

        // <summary>
        // Initializes a new instance of the TGCVector2 class.
        // </summary>
        //public TGCVector2();

        /// <summary>
        /// Initializes a new instance of the TGCVector2 class.
        /// </summary>
        /// <param name="valueX">Initial X value.</param>
        /// <param name="valueY">Initial Y value.</param>
        public TGCVector2(float valueX, float valueY)
        {
            this.X = valueX;
            this.Y = valueY;
            this.DXVector2 = new Vector2(valueX, valueY);
        }

        /// <summary>
        /// Initializes a new instance of the TGCVector2 class.
        /// </summary>
        /// <param name="dxVector2">Vector2 from value.</param>        
        public TGCVector2(Vector2 dxVector2)
        {
            this.X = dxVector2.X;
            this.Y = dxVector2.Y;
            this.DXVector2 = dxVector2;
        }

        /// <summary>
        /// Retrieves or sets the DirectX of a 2-D vector.
        /// </summary>
        public Vector2 DXVector2 { get; set; }

        /// <summary>
        /// Retrieves or sets the x component of a 2-D vector.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Retrieves or sets the y component of a 2-D vector.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Retrieves a 2-D vector (0,0).
        /// </summary>
        public static TGCVector2 Empty { get { return ZERO; } }

        /// <summary>
        /// Retrieves a 2-D vector (1,1).
        /// </summary>
        public static TGCVector2 One { get { return ONE; } }

        /// <summary>
        /// Adds two 2-D vectors.
        /// </summary>
        /// <param name="v">Source TGCVector2.</param>
        public void Add(TGCVector2 v)
        {
            this.X += v.X;
            this.Y += v.Y;
            DXVector2 = new Vector2(this.X, this.Y);
        }

        /// <summary>
        /// Adds two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>Sum of the two source TGCVector2.</returns>
        public static TGCVector2 Add(TGCVector2 left, TGCVector2 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.DXVector2 += right.DXVector2;
            return left;
        }

        /// <summary>
        /// Returns a point in barycentric coordinates, using specified 2-D vectors.
        /// </summary>
        /// <param name="v1">Source TGCVector2.</param>
        /// <param name="v2">Source TGCVector2.</param>
        /// <param name="v3">Source TGCVector2.</param>
        /// <param name="f">Weighting factor.</param>
        /// <param name="g">Weighting factor.</param>
        /// <returns>A TGCVector2 in barycentric coordinates.</returns>
        public static TGCVector2 BaryCentric(TGCVector2 v1, TGCVector2 v2, TGCVector2 v3, float f, float g)
        {
            return new TGCVector2(Vector2.BaryCentric(v1.ToVector2(), v2.ToVector2(), v3.ToVector2(), f, g));
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using specified 2-D vectors.
        /// </summary>
        /// <param name="position1">Source TGCVector2 that is a position vector.</param>
        /// <param name="position2">Source TGCVector2 that is a position vector.</param>
        /// <param name="position3">Source TGCVector2 that is a position vector.</param>
        /// <param name="position4">Source TGCVector2 that is a position vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns></returns>
        public static TGCVector2 CatmullRom(TGCVector2 position1, TGCVector2 position2, TGCVector2 position3,
            TGCVector2 position4, float weightingFactor)
        {
            return new TGCVector2(Vector2.CatmullRom(position1.ToVector2(), position2.ToVector2(), position3.ToVector2(), position4.ToVector2(), weightingFactor));
        }

        /// <summary>
        /// Returns the z component by calculating the cross product of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>The z component.</returns>
        public static float Ccw(TGCVector2 left, TGCVector2 right)
        {
            return Vector2.Ccw(left.ToVector2(), right.ToVector2());
        }

        /// <summary>
        /// Determines the dot product of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>Dot product.</returns>
        public static float Dot(TGCVector2 left, TGCVector2 right)
        {
            return Vector2.Dot(left.ToVector2(), right.ToVector2());
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="compare">Object with which to make the comparison.</param>
        /// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
        public override bool Equals(object compare)
        {
            if (compare is TGCVector2)
            {
                TGCVector2 other = ( (TGCVector2)compare);
                return (X == other.X) && (Y == other.Y);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            //TODO verificar correctamente esto en C#
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;                
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();            
                return hash;
            }            
        }

        /// <summary>
        /// Performs a Hermite spline interpolation using specified 2-D vectors.
        /// </summary>
        /// <param name="position">Source TGCVector2 that is a position vector.</param>
        /// <param name="tangent">Source TGCVector2 that is a tangent vector.</param>
        /// <param name="position2">Source TGCVector2 that is a position vector.</param>
        /// <param name="tangent2">Source TGCVector2 that is a tangent vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns>A TGCVector2 that is the result of the Hermite spline interpolation.</returns>
        public static TGCVector2 Hermite(TGCVector2 position, TGCVector2 tangent, TGCVector2 position2,
            TGCVector2 tangent2, float weightingFactor)
        {
            return new TGCVector2(Vector2.Hermite(position.ToVector2(), tangent.ToVector2(), position2.ToVector2(), tangent2.ToVector2(), weightingFactor));
        }

        /// <summary>
        /// Returns the length of a 2-D vector.
        /// </summary>
        /// <returns>Vector length.</returns>
        public float Length()
        {
            return DXVector2.Length();
        }

        /// <summary>
        /// Returns the length of a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <returns>Vector length.</returns>
        public static float Length(TGCVector2 source)
        {
            return Vector2.Length(source.ToVector2());
        }

        /// <summary>
        /// Returns the square of the length of a 2-D vector.
        /// </summary>
        /// <returns>Vector's squared length.</returns>
        public float LengthSq()
        {
            return DXVector2.LengthSq();
        }

        /// <summary>
        /// Returns the square of the length of a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <returns>Vector's squared length.</returns>
        public static float LengthSq(TGCVector2 source)
        {
            return Vector2.LengthSq(source.ToVector2());
        }

        /// <summary>
        /// Performs a linear interpolation between two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <param name="interpolater">Parameter that linearly interpolates between the vectors.</param>
        /// <returns></returns>
        public static TGCVector2 Lerp(TGCVector2 left, TGCVector2 right, float interpolater)
        {
            return new TGCVector2(Vector2.Lerp(left.ToVector2(), right.ToVector2(), interpolater));
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the largest components of two 2-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        public TGCVector2 Maximize(TGCVector2 source)
        {
            //TODO validar cambio de firma. este no retornaba nada.
            return new TGCVector2(Vector2.Maximize(this.ToVector2(), source.ToVector2())); 
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the largest components of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>A TGCVector2 structure that is made up of the largest components of the two vectors.</returns>
        public static TGCVector2 Maximize(TGCVector2 left, TGCVector2 right)
        {
            return new TGCVector2(Vector2.Maximize(left.ToVector2(), right.ToVector2()));
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the smallest components of two 2-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        public TGCVector2 Minimize(TGCVector2 source)
        {
            return new TGCVector2(Vector2.Minimize(this.ToVector2(), source.ToVector2()));
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the smallest components of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>A TGCVector2 that is made up of the smallest components of the two vectors.</returns>
        public static TGCVector2 Minimize(TGCVector2 left, TGCVector2 right)
        {
            return new TGCVector2(Vector2.Minimize(left.ToVector2(), right.ToVector2()));
        }

        /// <summary>
        /// Multiplies the current 2-D vector with a single value.
        /// </summary>
        /// <param name="s">Source float value.</param>
        public void Multiply(float s)
        {
            this.DXVector2.Multiply(s);
            this.X = DXVector2.X;
            this.Y = DXVector2.Y;            
        }

        /// <summary>
        /// Multiplies the current 2-D vector with a single value.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="s">Source float value.</param>
        /// <returns>A TGCVector2 that is the result of the source parameter multiplied by the s parameter.</returns>
        public static TGCVector2 Multiply(TGCVector2 source, float s)
        {
            source.X *= s;
            source.Y *= s;
            source.DXVector2.Multiply(s);
            return source;
        }

        /// <summary>
        /// Returns the normalized version of a 2-D vector.
        /// </summary>
        public void Normalize()
        {
            this.DXVector2 = Vector2.Normalize(this.ToVector2());
            this.X = DXVector2.X;
            this.Y = DXVector2.Y;
        }


        /// <summary>
        /// Returns the normalized version of a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <returns>A Vector2 that is the normalized version of the vector.</returns>
        public static TGCVector2 Normalize(TGCVector2 source)
        {
            return new TGCVector2(Vector2.Normalize(source.ToVector2()));
        }

        /// <summary>
        /// Adds two 2-D vectors.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the addition operator.</param>
        /// <param name="right">The TGCVector2 to the right of the addition operator.</param>
        /// <returns>Resulting TGCVector2.</returns>
        public static TGCVector2 operator +(TGCVector2 left, TGCVector2 right)
        {
            return TGCVector2.Add(left, right);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the equality operator.</param>
        /// <param name="right">The TGCVector2 to the right of the equality operator.</param>
        /// <returns>Value that is true if the objects are the same, or false if they are different.</returns>
        public static bool operator ==(TGCVector2 left, TGCVector2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the inequality operator.</param>
        /// <param name="right">The TGCVector2 to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCVector2 left, TGCVector2 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines the product of a single value and a 2-D vector.
        /// </summary>
        /// <param name="right">Source float structure.</param>
        /// <param name="left">Source TGCVector2.</param>
        /// <returns>A TGCVector2 structure that is the product of the right and left parameters.</returns>
        public static TGCVector2 operator *(float right, TGCVector2 left)
        {
            return TGCVector2.Multiply(left, right);
        }

        /// <summary>
        /// Determines the product of a single value and a 2-D vector.
        /// </summary>
        /// /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source float structure.</param>
        /// <returns>A TGCVector2 that is the product of the right and left parameters.</returns>
        public static TGCVector2 operator *(TGCVector2 left, float right)
        {
            return TGCVector2.Multiply(left, right);
        }

        /// <summary>
        /// Subtracts two 2-D vectors.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the subtraction operator.</param>
        /// <param name="right">The TGCVector2 to the right of the subtraction operator.</param>
        /// <returns>Resulting TGCVector2.</returns>
        public static TGCVector2 operator -(TGCVector2 left, TGCVector2 right)
        {
            return TGCVector2.Add(left, -right);
        }

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="vec">Source TGCVector2.</param>
        /// <returns>The TGCVector2 structure that is the result of the negation operation.</returns>
        public static TGCVector2 operator -(TGCVector2 vec)
        {
            //TODO asi o mejor el *-1???
            return new TGCVector2(-vec.X, -vec.Y);
        }

        /// <summary>
        /// Scales a 2-D vector.
        /// </summary>
        /// <param name="scalingFactor">Scaling value.</param>
        public void Scale(float scalingFactor)
        {
            //TODO validar diferencia entre scale y multiply
            this.Multiply(scalingFactor);
        }

        /// <summary>
        /// Scales a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="scalingFactor">Scaling value.</param>
        /// <returns>A TGCVector2 that is the scaled vector.</returns>
        public static TGCVector2 Scale(TGCVector2 source, float scalingFactor)
        {
            return TGCVector2.Multiply(source, scalingFactor);
        }

        /// <summary>
        /// Subtracts two 2-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector2 to subtract from the current TGCVector2 instance.</param>
        public void Subtract(TGCVector2 source)
        {
            this.Add(-source);
        }

        /// <summary>
        /// Subtracts two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2 to the left of the subtraction operator.</param>
        /// <param name="right">Source TGCVector2 to the right of the subtraction operator.</param>
        /// <returns>A TGCVector2 that is the result of the operation.</returns>
        public static TGCVector2 Subtract(TGCVector2 left, TGCVector2 right)
        {
            return TGCVector2.Add(left, -right);
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return "[" + string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", X) + "," +
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", Y) + "]";            
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A TGCVector4 structure that is the result of the method.</returns>
        public static TGCVector4 Transform(TGCVector2 source, TGCMatrix sourceMatrix)
        {
            return new TGCVector4(Vector2.Transform(source.ToVector2(), sourceMatrix.ToMatrix()));
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix.
        /// </summary>
        /// <param name="vector">Array of source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of Vector4 structures that are the result of the method.</returns>
        public static TGCVector4[] Transform(TGCVector2[] vector, TGCMatrix sourceMatrix)
        {
            TGCVector4[] ret = new TGCVector4[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                ret[i] = new TGCVector4(Vector2.Transform(vector[i].ToVector2(), sourceMatrix.ToMatrix()));
            }
            return ret;
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        public void TransformCoordinate(TGCMatrix sourceMatrix)
        {
            this.DXVector2 = Vector2.TransformCoordinate(this.ToVector2(), sourceMatrix.ToMatrix());
            this.X = DXVector2.X;
            this.Y = DXVector2.Y;
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A TGCVector2 that represents the results of the method.</returns>
        public static TGCVector2 TransformCoordinate(TGCVector2 source, TGCMatrix sourceMatrix)
        {
            return new TGCVector2(Vector2.TransformCoordinate(source.ToVector2(), sourceMatrix.ToMatrix()));
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="vector">Array of source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of TGCVector2 that represent the results of the method.</returns>
        public static TGCVector2[] TransformCoordinate(TGCVector2[] vector, TGCMatrix sourceMatrix)
        {
            TGCVector2[] ret = new TGCVector2[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                ret[i] = new TGCVector2(Vector2.TransformCoordinate(vector[i].ToVector2(), sourceMatrix.ToMatrix()));
            }
            return ret;
        }

        /// <summary>
        /// Transforms the 2-D vector normal by a given matrix.
        /// </summary>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        public void TransformNormal(TGCMatrix sourceMatrix)
        {
            this.DXVector2 = Vector2.TransformNormal(this.ToVector2(), sourceMatrix.ToMatrix());
            this.X = DXVector2.X;
            this.Y = DXVector2.Y;
        }

        /// <summary>
        /// Transforms the 2-D vector normal by a given matrix.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A TGCVector2 that contains the results of this method.</returns>
        public static TGCVector2 TransformNormal(TGCVector2 source, TGCMatrix sourceMatrix)
        {
            return new TGCVector2(Vector2.TransformNormal(source.ToVector2(), sourceMatrix.ToMatrix()));
        }

        /// <summary>
        /// Transforms the 2-D vector normal by a given matrix.
        /// </summary>
        /// <param name="vector">Array of source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of TGCVector2 that contain the results of this method.</returns>
        public static TGCVector2[] TransformNormal(TGCVector2[] vector, TGCMatrix sourceMatrix)
        {
            TGCVector2[] ret = new TGCVector2[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                ret[i] = new TGCVector2(Vector2.TransformNormal(vector[i].ToVector2(), sourceMatrix.ToMatrix()));
            }
            return ret;
        }

        /// <summary>
        /// Get the DX Vector3 wrapped to be use in DX primitives.
        /// </summary>
        /// <returns>The DX Vector3 wrapped</returns>
        public Vector2 ToVector2()
        {
            return this.DXVector2;
        }

        /// <summary>
        /// Transform TGCVector2[] to DX Vector2[]
        /// </summary>
        /// <param name="am">Source TGCVector2.</param>
        /// <returns>A Vector2[] with all de wrapped Matrix in TGCVector2.</returns>
        public static Vector2[] ToVector2Array(TGCVector2[] am)
        {
            Vector2[] m = new Vector2[am.Length];

            for (int i = 0; i < am.Length; i++)
            {
                m[i] = am[i].ToVector2();
            }

            return m;
        }

        #region Old TGCVectorUtils

        /// <summary>
        ///     Imprime un TGCVector2 de la forma [150.0,150.0]
        /// </summary>
        /// <returns></returns>
        public static string PrintVector2(float x, float y)
        {
            return "[" + TgcParserUtils.printFloat(x) + "," + TgcParserUtils.printFloat(y) + "]";
        }

        /// <summary>
        ///     Imprime un TGCVector2 de la forma [150.0,150.0]
        /// </summary>
        public static string PrintVector2(TGCVector2 vec)
        {
            return PrintVector2(vec.X, vec.Y);
        }

        /// <summary>
        ///     Imprime un TGCVector2 de la forma [150.0,150.0], tomando valores string
        /// </summary>
        public static string PrintVector2FromString(string x, string y)
        {
            return PrintVector2(TgcParserUtils.parseFloat(x), TgcParserUtils.parseFloat(y));
        }

        /// <summary>
        ///     Convierte un TGCVector2 a un float[2]
        /// </summary>
        public static float[] Vector2ToFloat2Array(TGCVector2 v)
        {
            return new[] { v.X, v.Y };
        }

        /// <summary>
        ///     Convierte un array de TGCVector2 a un array de float
        /// </summary>
        public static float[] Vector2ArrayToFloat2Array(TGCVector2[] values)
        {
            var data = new float[values.Length * 2];
            for (var i = 0; i < values.Length; i++)
            {
                data[i * 2] = values[i].X;
                data[i * 2 + 1] = values[i].Y;
            }
            return data;
        }

        #endregion
    }
}