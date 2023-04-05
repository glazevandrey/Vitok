namespace web_server.Models
{
    public static class Constants
    {
        #region Общие уведомления для репетитора и студента
        public const string NOTIF_START_LESSON = "У вас началось занятие!"; // в момент наступления занятия
        public const string NOTIF_TOMORROW_LESSON = "Завтра у вас занятие!"; // за день + на почту
        #endregion

        #region Уведомления Репетитора
        public const string NOTIF_DONT_FORGET_SET_STATUS = "Не забудьте отметить статус занятия!"; // окончание занятия
        public const string NOTIF_NEW_STUDENT_FOR_TUTOR = "У вас появился новый ученик {name}"; // новый ученик записался
        public const string NOTIF_NEW_LESSON_TUTOR = "У вас новое занятие с учеником {name} на {date}"; // новый ученик записался


        #endregion

        #region Уведомления Ученика
        public const string NOTIF_ONE_LESSON_LEFT = "У вас осталось одно занятие! Не забудьте пополнить баланс!"; // когда у ученика осталось 1 занятие
        public const string NOTIF_ZERO_LESSONS_LEFT = "У вас осталось 0 занятий! Не забудьте пополнить баланс!"; // когда у ученика осталось 0 занятия
        public const string NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT = "Внимание! Разовый перенос занятия с репетитором {name} с {dateOld} на {dateNew}"; // перенос
        public const string NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT_REGULAR = "Внимание! Постоянный перенос занятия с репетитором {name} с {dateOld} на {dateNew}"; // перенос
        public const string NOTIF_TUTOR_REJECT_USER_FUSER = "Репетитор {tutorName} больше не ведёт у Вас занятия!";

        #endregion

        #region Уведомления менеджера
        public const string NOTIF_REGULAR_RESCHEDULE = "Постоянный перенос занятия: {tutorName} с {studentName} с {oldDate} на {newDate}!"; // постоянный перенос
        public const string NOTIF_RESCHEDULE = "Разовый перенос занятия: {tutorName} с {studentName} с {oldDate} на {newDate}!"; // разовый перенос
        public const string NOTIF_NEW_STUDENT_FOR_MANAGER = "Новый ученик: {studentName} теперь занимается с {tutorName}"; // новый ученик записался
        public const string NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER = "0 занятий: напомните ученику {name} пополнить баланс!"; // 0 занятий у ученика
        public const string NOTIF_NEW_LESSON = "Добавлено {type} занятие: {studentName} занимается с {tutorName} {date}"; // добавлено занятие
        public const string NOTIF_REMOVE_LESSON = "Удалено {type} занятие: {tutorName} с {studentName} в {date}"; // удалено занятие
        public const string NOTIF_TUTOR_REJECT_USER_FMANAGER = "Репетитор {tutorName} отказался от ученика {userName}!";

        #endregion
    }
}
