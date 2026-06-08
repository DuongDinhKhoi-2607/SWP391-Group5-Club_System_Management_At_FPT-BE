using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

public partial class ClubSystemDbContext : DbContext
{
    public ClubSystemDbContext()
    {
    }

    public ClubSystemDbContext(DbContextOptions<ClubSystemDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Boardmember> Boardmembers { get; set; }

    public virtual DbSet<Club> Clubs { get; set; }

    public virtual DbSet<Clubboard> Clubboards { get; set; }

    public virtual DbSet<Clubreport> Clubreports { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Documenttype> Documenttypes { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Evidence> Evidences { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Reportperiod> Reportperiods { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userinformation> Userinformations { get; set; }

   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<Boardmember>(entity =>
        {
            entity.HasKey(e => e.Boardmemberid).HasName("boardmember_pkey");

            entity.ToTable("boardmember");

            entity.HasIndex(e => e.Boardid, "ix_boardmember_boardid");

            entity.HasIndex(e => e.Membershipid, "ix_boardmember_membershipid");

            entity.Property(e => e.Boardmemberid).HasColumnName("boardmemberid");
            entity.Property(e => e.Appointedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("appointedat");
            entity.Property(e => e.Boardid).HasColumnName("boardid");
            entity.Property(e => e.Dutydescription)
                .HasMaxLength(500)
                .HasColumnName("dutydescription");
            entity.Property(e => e.Handoverdocumenturl)
                .HasMaxLength(500)
                .HasColumnName("handoverdocumenturl");
            entity.Property(e => e.KpiScore)
                .HasPrecision(5, 2)
                .HasColumnName("kpi_score");
            entity.Property(e => e.Membershipid).HasColumnName("membershipid");
            entity.Property(e => e.Position)
                .HasMaxLength(50)
                .HasColumnName("position");

            entity.HasOne(d => d.Board).WithMany(p => p.Boardmembers)
                .HasForeignKey(d => d.Boardid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_boardmember_clubboard");

            entity.HasOne(d => d.Membership).WithMany(p => p.Boardmembers)
                .HasForeignKey(d => d.Membershipid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_boardmember_membership");
        });

        modelBuilder.Entity<Club>(entity =>
        {
            entity.HasKey(e => e.Clubid).HasName("club_pkey");

            entity.ToTable("club");

            entity.HasIndex(e => e.Clubcode, "club_clubcode_key").IsUnique();

            entity.HasIndex(e => e.Clubname, "club_clubname_key").IsUnique();

            entity.Property(e => e.Clubid).HasColumnName("clubid");
            entity.Property(e => e.Clubcode)
                .HasMaxLength(50)
                .HasColumnName("clubcode");
            entity.Property(e => e.Clubname)
                .HasMaxLength(200)
                .HasColumnName("clubname");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Fanpageurl)
                .HasMaxLength(500)
                .HasColumnName("fanpageurl");
            entity.Property(e => e.Foundeddate).HasColumnName("foundeddate");
            entity.Property(e => e.Logoimage)
                .HasMaxLength(500)
                .HasColumnName("logoimage");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Đang hoạt động'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Totalactivemembers).HasColumnName("totalactivemembers");
        });

        modelBuilder.Entity<Clubboard>(entity =>
        {
            entity.HasKey(e => e.Boardid).HasName("clubboard_pkey");

            entity.ToTable("clubboard");

            entity.HasIndex(e => e.Clubid, "ix_clubboard_clubid");

            entity.Property(e => e.Boardid).HasColumnName("boardid");
            entity.Property(e => e.Boardname)
                .HasMaxLength(200)
                .HasColumnName("boardname");
            entity.Property(e => e.Clubid).HasColumnName("clubid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Đang đương nhiệm'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Club).WithMany(p => p.Clubboards)
                .HasForeignKey(d => d.Clubid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubboard_club");
        });

        modelBuilder.Entity<Clubreport>(entity =>
        {
            entity.HasKey(e => e.Clubreportid).HasName("clubreport_pkey");

            entity.ToTable("clubreport");

            entity.HasIndex(e => e.Clubid, "ix_clubreport_clubid");

            entity.HasIndex(e => e.Reportperiodid, "ix_clubreport_reportperiodid");

            entity.Property(e => e.Clubreportid).HasColumnName("clubreportid");
            entity.Property(e => e.Clubid).HasColumnName("clubid");
            entity.Property(e => e.Financialbalance)
                .HasPrecision(18, 2)
                .HasColumnName("financialbalance");
            entity.Property(e => e.IcpdpFeedback).HasColumnName("icpdp_feedback");
            entity.Property(e => e.Reportperiodid).HasColumnName("reportperiodid");
            entity.Property(e => e.Reporttitle)
                .HasMaxLength(255)
                .HasColumnName("reporttitle");
            entity.Property(e => e.Reviewedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reviewedat");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chờ duyệt'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Submittedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("submittedat");
            entity.Property(e => e.Summarycontent).HasColumnName("summarycontent");
            entity.Property(e => e.Totaleventsheld).HasColumnName("totaleventsheld");

            entity.HasOne(d => d.Club).WithMany(p => p.Clubreports)
                .HasForeignKey(d => d.Clubid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubreport_club");

            entity.HasOne(d => d.Reportperiod).WithMany(p => p.Clubreports)
                .HasForeignKey(d => d.Reportperiodid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubreport_reportperiod");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Departmentid).HasName("department_pkey");

            entity.ToTable("department");

            entity.HasIndex(e => e.Departmentname, "department_departmentname_key").IsUnique();

            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Departmentname)
                .HasMaxLength(200)
                .HasColumnName("departmentname");
            entity.Property(e => e.Officelocation)
                .HasMaxLength(255)
                .HasColumnName("officelocation");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Hoạt động'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Documentid).HasName("document_pkey");

            entity.ToTable("document");

            entity.HasIndex(e => e.Clubid, "ix_document_clubid");

            entity.HasIndex(e => e.Documenttypeid, "ix_document_documenttypeid");

            entity.Property(e => e.Documentid).HasColumnName("documentid");
            entity.Property(e => e.Accesslevel)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Nội bộ'::character varying")
                .HasColumnName("accesslevel");
            entity.Property(e => e.Clubid).HasColumnName("clubid");
            entity.Property(e => e.Documentname)
                .HasMaxLength(255)
                .HasColumnName("documentname");
            entity.Property(e => e.Documenttypeid).HasColumnName("documenttypeid");
            entity.Property(e => e.Downloadcount).HasColumnName("downloadcount");
            entity.Property(e => e.Eventid).HasColumnName("eventid");
            entity.Property(e => e.Filesize).HasColumnName("filesize");
            entity.Property(e => e.Fileurl)
                .HasMaxLength(500)
                .HasColumnName("fileurl");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");

            entity.HasOne(d => d.Club).WithMany(p => p.Documents)
                .HasForeignKey(d => d.Clubid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_document_club");

            entity.HasOne(d => d.Documenttype).WithMany(p => p.Documents)
                .HasForeignKey(d => d.Documenttypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_document_documenttype");

            entity.HasOne(d => d.Event).WithMany(p => p.Documents)
                .HasForeignKey(d => d.Eventid)
                .HasConstraintName("fk_document_event");
        });

        modelBuilder.Entity<Documenttype>(entity =>
        {
            entity.HasKey(e => e.Documenttypeid).HasName("documenttype_pkey");

            entity.ToTable("documenttype");

            entity.HasIndex(e => e.Typename, "documenttype_typename_key").IsUnique();

            entity.Property(e => e.Documenttypeid).HasColumnName("documenttypeid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Typename)
                .HasMaxLength(150)
                .HasColumnName("typename");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Eventid).HasName("event_pkey");

            entity.ToTable("event");

            entity.HasIndex(e => e.Clubid, "ix_event_clubid");

            entity.Property(e => e.Eventid).HasColumnName("eventid");
            entity.Property(e => e.Actualparticipants).HasColumnName("actualparticipants");
            entity.Property(e => e.Clubid).HasColumnName("clubid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Endtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("endtime");
            entity.Property(e => e.Eventname)
                .HasMaxLength(255)
                .HasColumnName("eventname");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Planbudget)
                .HasMaxLength(500)
                .HasColumnName("planbudget");
            entity.Property(e => e.Starttime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("starttime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Lập kế hoạch'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Targetparticipants).HasColumnName("targetparticipants");

            entity.HasOne(d => d.Club).WithMany(p => p.Events)
                .HasForeignKey(d => d.Clubid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_event_club");
        });

        modelBuilder.Entity<Evidence>(entity =>
        {
            entity.HasKey(e => e.Evidenceid).HasName("evidence_pkey");

            entity.ToTable("evidence");

            entity.HasIndex(e => e.Participantid, "ix_evidence_participantid");

            entity.Property(e => e.Evidenceid).HasColumnName("evidenceid");
            entity.Property(e => e.Evidencename)
                .HasMaxLength(255)
                .HasColumnName("evidencename");
            entity.Property(e => e.Fileurl)
                .HasMaxLength(500)
                .HasColumnName("fileurl");
            entity.Property(e => e.Isverified)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Đang chờ'::character varying")
                .HasColumnName("isverified");
            entity.Property(e => e.Participantid).HasColumnName("participantid");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");
            entity.Property(e => e.Verifiedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("verifiedat");

            entity.HasOne(d => d.Participant).WithMany(p => p.Evidences)
                .HasForeignKey(d => d.Participantid)
                .HasConstraintName("fk_evidence_participant");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.Membershipid).HasName("membership_pkey");

            entity.ToTable("membership");

            entity.HasIndex(e => e.Clubid, "ix_membership_clubid");

            entity.HasIndex(e => e.Userid, "ix_membership_userid");

            entity.Property(e => e.Membershipid).HasColumnName("membershipid");
            entity.Property(e => e.Clubid).HasColumnName("clubid");
            entity.Property(e => e.Joindate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("joindate");
            entity.Property(e => e.Joinreason)
                .HasMaxLength(500)
                .HasColumnName("joinreason");
            entity.Property(e => e.Leftdate).HasColumnName("leftdate");
            entity.Property(e => e.Personalgoal)
                .HasMaxLength(500)
                .HasColumnName("personalgoal");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Đang sinh hoạt'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Club).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.Clubid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_membership_club");

            entity.HasOne(d => d.User).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_membership_user");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Participantid).HasName("participant_pkey");

            entity.ToTable("participant");

            entity.HasIndex(e => e.Eventid, "ix_participant_eventid");

            entity.HasIndex(e => e.Userid, "ix_participant_userid");

            entity.HasIndex(e => new { e.Eventid, e.Userid }, "uq_participant_event_user").IsUnique();

            entity.Property(e => e.Participantid).HasColumnName("participantid");
            entity.Property(e => e.Attendancestatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Vắng mặt'::character varying")
                .HasColumnName("attendancestatus");
            entity.Property(e => e.Checkedinat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("checkedinat");
            entity.Property(e => e.Evaluationscore)
                .HasPrecision(4, 2)
                .HasColumnName("evaluationscore");
            entity.Property(e => e.Eventid).HasColumnName("eventid");
            entity.Property(e => e.Feedback)
                .HasMaxLength(1000)
                .HasColumnName("feedback");
            entity.Property(e => e.Roleinevent)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Thành viên tham gia'::character varying")
                .HasColumnName("roleinevent");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Event).WithMany(p => p.Participants)
                .HasForeignKey(d => d.Eventid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_participant_event");

            entity.HasOne(d => d.User).WithMany(p => p.Participants)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_participant_user");
        });

        modelBuilder.Entity<Reportperiod>(entity =>
        {
            entity.HasKey(e => e.Reportperiodid).HasName("reportperiod_pkey");

            entity.ToTable("reportperiod");

            entity.HasIndex(e => e.Semesterid, "ix_reportperiod_semesterid");

            entity.Property(e => e.Reportperiodid).HasColumnName("reportperiodid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Deadline)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deadline");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Periodname)
                .HasMaxLength(150)
                .HasColumnName("periodname");
            entity.Property(e => e.Semesterid).HasColumnName("semesterid");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Mở cổng nhận'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Semester).WithMany(p => p.Reportperiods)
                .HasForeignKey(d => d.Semesterid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reportperiod_semester");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.Semesterid).HasName("semester_pkey");

            entity.ToTable("semester");

            entity.HasIndex(e => e.Semestername, "semester_semestername_key").IsUnique();

            entity.Property(e => e.Semesterid).HasColumnName("semesterid");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Semestername)
                .HasMaxLength(100)
                .HasColumnName("semestername");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Dự kiến'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Studentid).HasName("student_pkey");

            entity.ToTable("student");

            entity.HasIndex(e => e.Schoolemail, "student_schoolemail_key").IsUnique();

            entity.Property(e => e.Studentid)
                .HasColumnType("character varying")
                .HasColumnName("studentid");
            entity.Property(e => e.Academicbatch)
                .HasColumnType("character varying")
                .HasColumnName("academicbatch");
            entity.Property(e => e.Dateofbirth).HasColumnName("dateofbirth");
            entity.Property(e => e.Fullname)
                .HasColumnType("character varying")
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasColumnType("character varying")
                .HasColumnName("gender");
            entity.Property(e => e.Major)
                .HasColumnType("character varying")
                .HasColumnName("major");
            entity.Property(e => e.Schoolemail)
                .HasColumnType("character varying")
                .HasColumnName("schoolemail");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Đang học'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("appuser_pkey");

            entity.ToTable("user");

            entity.HasIndex(e => e.Username, "appuser_username_key").IsUnique();

            entity.HasIndex(e => e.Departmentid, "ix_user_departmentid");

            entity.Property(e => e.Userid)
                .HasDefaultValueSql("nextval('appuser_userid_seq'::regclass)")
                .HasColumnName("userid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Lastloginat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastloginat");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(500)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chờ kích hoạt'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Systemrole)
                .HasMaxLength(50)
                .HasColumnName("systemrole");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.Departmentid)
                .HasConstraintName("fk_user_department");
        });

        modelBuilder.Entity<Userinformation>(entity =>
        {
            entity.HasKey(e => e.Userinfoid).HasName("userinformation_pkey");

            entity.ToTable("userinformation");

            entity.HasIndex(e => e.Studentid, "userinformation_studentid_key").IsUnique();

            entity.HasIndex(e => e.Userid, "userinformation_userid_key").IsUnique();

            entity.Property(e => e.Userinfoid).HasColumnName("userinfoid");
            entity.Property(e => e.Avatar)
                .HasMaxLength(500)
                .HasColumnName("avatar");
            entity.Property(e => e.Graduationdate).HasColumnName("graduationdate");
            entity.Property(e => e.Infoupdatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("infoupdatedat");
            entity.Property(e => e.Isalumni).HasColumnName("isalumni");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(30)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Studentid)
                .HasMaxLength(50)
                .HasColumnName("studentid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Student).WithOne(p => p.Userinformation)
                .HasForeignKey<Userinformation>(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_userinformation_student");

            entity.HasOne(d => d.User).WithOne(p => p.Userinformation)
                .HasForeignKey<Userinformation>(d => d.Userid)
                .HasConstraintName("fk_userinformation_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
