import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Custom validators for Plaintiff forms matching backend FluentValidation rules.
 * Implements SRS UC 6.5.1 validation requirements.
 */
export class CustomValidators {
  // Identity type constants
  private static readonly IDENTITY_TYPE_NATIONAL_ID = 1;
  private static readonly IDENTITY_TYPE_RESIDENT_ID = 2;
  private static readonly IDENTITY_TYPE_PASSPORT = 3;

  // Arabic-only regex pattern
  private static readonly ARABIC_ONLY_PATTERN = /^[\u0600-\u06FF\s\d\-\.]+$/;

  /**
   * ERR014: National ID must start with 1 and be 10 digits.
   * ERR015: Resident ID must start with 2 and be 10 digits.
   * Passport: up to 20 characters.
   */
  static identityNumber(identityTypeIdGetter: () => number | null): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null; // Let required validator handle empty
      }

      const identityTypeId = identityTypeIdGetter();

      switch (identityTypeId) {
        case CustomValidators.IDENTITY_TYPE_NATIONAL_ID:
          if (!/^1\d{9}$/.test(value)) {
            return { identityNumber: 'الرقم المدخل ليس هوية مواطن - يجب أن يبدأ بـ 1 ويتكون من 10 أرقام' };
          }
          break;
        case CustomValidators.IDENTITY_TYPE_RESIDENT_ID:
          if (!/^2\d{9}$/.test(value)) {
            return { identityNumber: 'الرقم المدخل ليس هوية مقيم - يجب أن يبدأ بـ 2 ويتكون من 10 أرقام' };
          }
          break;
        case CustomValidators.IDENTITY_TYPE_PASSPORT:
          if (value.length > 20) {
            return { identityNumber: 'رقم جواز السفر يجب ألا يتجاوز 20 حرف' };
          }
          break;
      }

      return null;
    };
  }

  /**
   * Mobile number must start with 05 and be 10 digits total.
   */
  static mobileNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^05\d{8}$/.test(value)) {
        return { mobileNumber: 'رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام' };
      }

      return null;
    };
  }

  /**
   * Gender must be Arabic values: ذكر or أنثى
   */
  static gender(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^(ذكر|أنثى)$/.test(value)) {
        return { gender: 'الجنس يجب أن يكون \'ذكر\' أو \'أنثى\'' };
      }

      return null;
    };
  }

  /**
   * Arabic-only text validation.
   */
  static arabicOnly(fieldName: string = 'الحقل'): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!CustomValidators.ARABIC_ONLY_PATTERN.test(value)) {
        return { arabicOnly: `${fieldName} يجب أن يكون بالعربية فقط` };
      }

      return null;
    };
  }

  /**
   * Date must not be in the future.
   */
  static notFutureDate(fieldName: string = 'التاريخ'): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      const date = new Date(value);
      const today = new Date();
      today.setHours(23, 59, 59, 999);

      if (date > today) {
        return { notFutureDate: `${fieldName} لا يمكن أن يكون في المستقبل` };
      }

      return null;
    };
  }

  /**
   * End date must be after start date.
   */
  static dateAfter(startDateGetter: () => Date | string | null, fieldName: string = 'التاريخ'): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const endValue = control.value;
      if (!endValue) {
        return null;
      }

      const startValue = startDateGetter();
      if (!startValue) {
        return null;
      }

      const endDate = new Date(endValue);
      const startDate = new Date(startValue);

      if (endDate <= startDate) {
        return { dateAfter: `${fieldName} يجب أن يكون أكبر من تاريخ البداية` };
      }

      return null;
    };
  }

  /**
   * Commercial Registration Number: exactly 10 digits.
   */
  static commercialRegNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^\d{10}$/.test(value)) {
        return { commercialRegNumber: 'رقم السجل التجاري يجب أن يكون 10 أرقام فقط' };
      }

      return null;
    };
  }

  /**
   * License Number: exactly 10 digits.
   */
  static licenseNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^\d{10}$/.test(value)) {
        return { licenseNumber: 'رقم الترخيص يجب أن يكون 10 أرقام فقط' };
      }

      return null;
    };
  }

  /**
   * Court Deed Number: exactly 10 digits.
   */
  static courtDeedNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^\d{10}$/.test(value)) {
        return { courtDeedNumber: 'رقم صك المحكمة يجب أن يكون 10 أرقام فقط' };
      }

      return null;
    };
  }

  /**
   * Waqf Oversight Type: خاصة or حكومية
   */
  static waqfOversightType(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^(خاصة|حكومية)$/.test(value)) {
        return { waqfOversightType: 'نظارة الوقف يجب أن تكون \'خاصة\' أو \'حكومية\'' };
      }

      return null;
    };
  }

  /**
   * Building Number: exactly 4 digits.
   */
  static buildingNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^\d{4}$/.test(value)) {
        return { buildingNumber: 'رقم المبنى يجب أن يكون 4 أرقام فقط' };
      }

      return null;
    };
  }

  /**
   * Postal Code: exactly 5 digits.
   */
  static postalCode(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^\d{5}$/.test(value)) {
        return { postalCode: 'الرمز البريدي يجب أن يكون 5 أرقام فقط' };
      }

      return null;
    };
  }

  /**
   * Additional Number: exactly 4 digits.
   */
  static additionalNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      if (!/^\d{4}$/.test(value)) {
        return { additionalNumber: 'الرقم الإضافي يجب أن يكون 4 أرقام فقط' };
      }

      return null;
    };
  }
}
