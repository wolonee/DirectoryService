export type ApiError = {
    code: string;
    message: string;
    invalidField?: string | null;
    type: ErrorType;
}

// export type ErrorMessage = {
    // code: string;
    // message: string;
    // invalidField?: string | null;
// }

export type ErrorType = 
    | "validation"
    | "not_found"
    | "failure"
    | "conflict"
    | "authentification"
    | "authorization";


export class EnvelopeError extends Error {
  public readonly errorList: ApiError[];
  public readonly type: ErrorType;

  constructor(errorList: ApiError[]) {
    const safeErrorList: ApiError[] =
        errorList.length > 0
        ? errorList
        : [
            {
                code: "unknown.error",
                message: "Unknown error",
                type: "failure",
            },
        ];

    const firstError = safeErrorList[0];

    super(firstError.message);

    this.name = "EnvelopeError";
    this.errorList = safeErrorList;
    this.type = firstError.type;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }

  get firstError(): ApiError {
    return this.errorList[0];
  }

  get fieldErrors(): ApiError[] {
    return this.errorList.filter((error) => error.invalidField);
  }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
    return error instanceof EnvelopeError
}