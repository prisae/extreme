#include <mkl_service.h>
#include <mkl.h>

#include <complex>
#include <iostream>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif


using namespace std;

typedef struct { double re; double im; } complex16;

namespace Native
{
	extern "C"
	{
		DllExport void ZgemvAsymTrans(int nz, MKL_Complex16* alpha, MKL_Complex16* beta, void* green, void* input, void* result)
		{
			cblas_zgemv(CblasRowMajor, CblasTrans, nz, nz, alpha, green, nz, input, 1, beta, result, 1);
		}

		DllExport void ZgemvAsymNoTrans(int nz, MKL_Complex16* alpha, MKL_Complex16* beta, void* green, void* input, void* result)
		{
			cblas_zgemv(CblasRowMajor, CblasNoTrans, nz, nz, alpha, green, nz, input, 1, beta, result, 1);
		}

		DllExport void ZgemvAtoO(int n, int m, MKL_Complex16* alpha, MKL_Complex16* beta, void* green, void* input, void* result)
		{
			cblas_zgemv(CblasRowMajor, CblasTrans, n, m, alpha, green, m, input, 1, beta, result, 1);
		}

		DllExport void ZgemvSym(int nz, MKL_Complex16* alpha, MKL_Complex16* beta, MKL_Complex16* green, MKL_Complex16* input, MKL_Complex16* result)
		{
			const char u = 'U';
			int one = 1;

			zspmv(&u, &nz, alpha, green, input, &one, beta, result, &one);
		}

		DllExport void FullZgemv(int nz, int length,
			MKL_Complex16* xx,
			MKL_Complex16* xy,
			MKL_Complex16* xz,
			MKL_Complex16* yy,
			MKL_Complex16* yz,
			MKL_Complex16* zz,
			MKL_Complex16* src,
			MKL_Complex16* dst)
		{
			int asymNz = nz * nz;
			int symmNz = nz + nz * (nz - 1) / 2;

			MKL_Complex16 one = { 1, 0 };
			MKL_Complex16 minusOne = { -1, 0 };
			MKL_Complex16 zero = { 0, 0 };

			int dataShift;
			int symmShift;
			int asymShift;

			MKL_Complex16* dstX;
			MKL_Complex16* dstY;
			MKL_Complex16* dstZ;

			MKL_Complex16* srcX;
			MKL_Complex16* srcY;
			MKL_Complex16* srcZ;

			//#pragma omp parallel  for private (dataShift, symmShift, asymShift, dstX, dstY, dstZ, srcX, srcY, srcZ)
			//#pragma parallel always
			for (int i = 0; i < length; i++)
			{
				dataShift = i * 3 * nz;
				symmShift = i * symmNz;
				asymShift = i * asymNz;

				dstX = dst + dataShift;
				dstY = dst + dataShift + nz;
				dstZ = dst + dataShift + nz + nz;

				srcX = src + dataShift;
				srcY = src + dataShift + nz;
				srcZ = src + dataShift + nz + nz;

				ZgemvSym(nz, &one, &zero, xx + symmShift, srcX, dstX);
				ZgemvSym(nz, &one, &one, xy + symmShift, srcY, dstX);
				ZgemvAsymNoTrans(nz, &one, &one, xz + asymShift, srcZ, dstX);

				ZgemvSym(nz, &one, &zero, xy + symmShift, srcX, dstY);
				ZgemvSym(nz, &one, &one, yy + symmShift, srcY, dstY);
				ZgemvAsymNoTrans(nz, &one, &one, yz + asymShift, srcZ, dstY);

				ZgemvAsymTrans(nz, &minusOne, &zero, xz + asymShift, srcX, dstZ);
				ZgemvAsymTrans(nz, &minusOne, &one, yz + asymShift, srcY, dstZ);
				ZgemvSym(nz, &one, &one, zz + symmShift, srcZ, dstZ);
			}
		}
	}
}