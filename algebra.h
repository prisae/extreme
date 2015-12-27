#ifdef MKL
#include <mkl_service.h>
#include <mkl.h>
#define complex_ptr MKL_Complex16*
#define complex_ptr2 MKL_Complex16*
#define blasint MKL_INT
#else
#include <cblas.h>
#include <f77blas.h>

#ifdef OPENBLAS_NEEDBUNDERSCORE
#define zspmv zspmv_
#define zrotg zrotg_
#define zrot zrot_
#endif
#define complex_ptr double*
#define complex_ptr2 openblas_complex_double*


#endif
#define one_int (blasint) 1
typedef struct { double re; double im; } complex16;
extern "C"{
void zrot_ (blasint *, double *, blasint *, double *, blasint *, double *, double *);
}
